using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Database.Direct;
using AutoGram.Helpers;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response.Direct;

namespace AutoGram.Task
{
    static class Direct
    {
        private static readonly HashSet<string> LocalUsernamesBlackList = new HashSet<string>();
        private static readonly object PhotoLocker = new object();
        private static readonly object BlackListLocker = new object();

        private static readonly List<int> _directWays = new List<int> { 6 };
        private static int _directWayCounter;
        private static readonly object _directWayLock = new object();

        public static void Do(Instagram.Instagram user)
        {
            int refreshThreadsDelay = Settings.Advanced.Direct.RefreshThreadsDelay;
            int refreshThreadsCount = Settings.Advanced.Direct.RefreshThreadsCount;

            user.Log("Open direct.");

            // Open direct threads
            var inboxResponse = user.Do(() => user.Direct.Inbox());

            // Save counters
            user.Activity.Direct.PendingRequests = inboxResponse.PendingRequestsTotal;
            user.Activity.Direct.UnseenMessages = inboxResponse.Inbox.UnseenCount;

            ShowInboxState(user, inboxResponse);

            int refreshCounter = 0;
            bool firstRefresh = true;

            while (true)
            {
                if (!firstRefresh)
                {
                    inboxResponse = user.Do(() => user.Direct.Inbox());
                    ShowInboxState(user, inboxResponse);
                }
                else firstRefresh = false;

                if (Settings.Advanced.Direct.CheckInboxOnly)
                    break;

                var allThreads = new List<ThreadModel>();

                // Get unseen threads
                if (inboxResponse.Inbox.UnseenCount > 0)
                {
                    var unseenThreads = GetUnseenThreads(user, inboxResponse);
                    allThreads.AddRange(unseenThreads);
                }

                // Get pending threads
                if (inboxResponse.PendingRequestsTotal > 0)
                {
                    var pendingThreads = GetPendingThreads(user);
                    allThreads.AddRange(pendingThreads);
                }

                if (!allThreads.Any() && !user.DirectRepository.Any())
                    break;

                if (!allThreads.Any() && refreshCounter == 0)
                    break;

                if (allThreads.Count >= 2)
                {
                    refreshCounter--;
                }

                // Process all threads
                ProcessThreads(user, allThreads);

                refreshCounter++;

                if (refreshCounter >= refreshThreadsCount) break;

                user.Log($"Refresh direct {refreshThreadsDelay} s.");
                Thread.Sleep(refreshThreadsDelay * 1000);
            }
        }

        private static void ProcessThreads(Instagram.Instagram user, List<ThreadModel> threads)
        {
            foreach (var thread in threads)
            {
                if (thread.IsUnseenDirectStory)
                {
                    SeeVisualThreadItems(user, thread);
                }

                ProcessThread(user, thread);
            }
        }

        private static void SeeVisualThreadItems(Instagram.Instagram user, ThreadModel thread)
        {
            if (!thread.IsUnseenDirectStory &&
                !Settings.Advanced.Direct.WatchVisualThreadItems) return;

            var visualThreadItemsResponse =
                user.Do(
                    () =>
                        user.Direct.ReturnUnseenVisualThreadItems(thread.ThreadId, thread.DirectStory.NewestCursor));

            user.Log($"Thread {thread.Users.FirstOrDefault()?.Username} have {visualThreadItemsResponse.UnseenCount} visual messages.");

            if (visualThreadItemsResponse.Items.All(v => v.ItemId != thread.DirectStory.Items.FirstOrDefault()?.ItemId))
            {
                visualThreadItemsResponse.Items.Add(thread.DirectStory.Items.FirstOrDefault());
            }

            while (true)
            {
                MarkSeenVisualThreadItems(user, thread, visualThreadItemsResponse.Items);

                visualThreadItemsResponse =
                    user.Do(
                        () =>
                            user.Direct.ReturnUnseenVisualThreadItems(thread.ThreadId, visualThreadItemsResponse.NewestCursor));

                if (visualThreadItemsResponse.UnseenCount == 0 || !visualThreadItemsResponse.Items.Any())
                {
                    break;
                }
            }
        }

        private static void MarkSeenVisualThreadItems(Instagram.Instagram user, ThreadModel thread, List<ThreadItemModel> items)
        {
            foreach (var item in items)
            {
                user.Log($"Marks seen visual item #{item.ItemId}");

                user.Do(
                    () =>
                        user.Direct.MarkSeenVisualThreadItem(thread.ThreadId, item.ItemId));

                Utils.RandomSleep(3000, 5000);
            }
        }

        private static void ProcessThread(Instagram.Instagram user, ThreadModel thread)
        {
            int refreshCount = 0;

            while (true)
            {
                user.Log(refreshCount == 0
                    ? $"Open thread {thread.Username}"
                    : $"Refresh thread {thread.Username}");

                try
                {
                    thread = OpenThread(user, thread);
                }
                catch (DirectDeclineRequestException) { break; }

                // Initialize direct thread from database
                var directThread = InitThread(user, thread);
                var unreedMessages = SyncThread(user, directThread, thread);

                user.Log($"Unreed {unreedMessages.Count} messages in thread.");

                Utils.RandomSleep(2000, 5000);

                // Marks u seen
                if (thread.IsUnseenThread(user.AccountId))
                {
                    if (thread.IsUnreedMessages(user.AccountId))
                    {
                        if (thread.LastPermanentItem != null)
                        {
                            user.Log("Marks seen.");
                            user.Do(() => user.Direct.MarkSeen(thread.ThreadId, thread.LastPermanentItem.ItemId));
                        }
                        else
                        {
                            Log.Write($"thread.LastPermanentItem is null. Direct.cs, 178", LogResource.Special);
                        }
                    }

                    // See visual threads
                    if (thread.IsUnseenDirectStory)
                    {
                        SeeVisualThreadItems(user, thread);
                    }
                    else if (unreedMessages.Any(m => m.IsUnseenDirectStory()))
                    {
                        var visualItems = unreedMessages.Where(m => m.IsUnseenDirectStory()).ToList();
                        MarkSeenVisualThreadItems(user, thread, visualItems);
                    }
                }

                // Send messages
                bool messagesSended = false;

                if (unreedMessages.Any())
                {
                    SendMessages(user, directThread, unreedMessages, out messagesSended);
                }

                if (directThread.IsFinishedDialog)
                {
                    if (messagesSended)
                    {
                        refreshCount++;
                        continue;
                    }

                    user.Log($"Dialog with {directThread.Username} was finished.");
                    break;
                }

                int delay = Settings.Advanced.Direct.RefreshThreadDelay;
                int refreshThreadCount = Settings.Advanced.Direct.RefreshThreadCount;

                user.Log($"Sleep {delay} s.");
                Thread.Sleep(delay * 1000);

                refreshCount++;
                if (refreshCount >= refreshThreadCount) break;
            }
        }

        private static void MessageProcessor(Instagram.Instagram user, DirectThread thread, List<ThreadItemModel> unreedMessages, out bool messagesSended)
        {
            messagesSended = false;
            if (thread.IsFinishedDialog) return;

            switch (thread.State)
            {
                case 0:

                    break;
            }

        }

        private static void SendMessages(Instagram.Instagram user, DirectThread thread, List<ThreadItemModel> unreedMessages, out bool messagesSended)
        {
            messagesSended = false;
            if (thread.IsFinishedDialog) return;

            // Some data from here was deleted
            user.DirectRepository.Update(thread);
        }

        private static void SendLinkMessage(Instagram.Instagram user, DirectThread thread, string linkUrl, string linkText)
        {
            int errorCount = 0;

            while (true)
            {
                var sendLinkResponse = user.Do(() => user.Direct.SendLink(linkText, linkUrl, thread.ThreadId));

                if (sendLinkResponse.IsOk()) break;

                if (sendLinkResponse.IsForbidden)
                {
                    user.Log($"Error: Failed sentry check.");
                }
                else
                {
                    string statusCode = !string.IsNullOrEmpty(sendLinkResponse.StatusCode)
                        ? sendLinkResponse.StatusCode
                        : "UnknownStatusCode";

                    string errorMessage = !string.IsNullOrEmpty(sendLinkResponse?.Payload.Message)
                        ? sendLinkResponse?.Payload.Message
                        : sendLinkResponse.IsMessage()
                            ? sendLinkResponse.Message
                            : "UnknownErrorMessage";

                    user.Log($"Error: status code: {statusCode} | message: {errorMessage}");
                    Log.Write($"Error: status code: {statusCode} | message: {errorMessage}", LogResource.Direct);
                }

                errorCount++;

                if (errorCount >= 2)
                {
                    Telegram.SendMessage($"Error {user.Username}: Failed sentry check.", TelegramNotification.ServerRoom);
                    break;
                }
            }
        }

        private static List<ThreadModel> GetUnseenThreads(Instagram.Instagram user, InboxResponse inboxResponse)
        {
            int unseenThreadsCount = inboxResponse.Inbox.UnseenCount;

            List<ThreadModel> unseenThreads = new List<ThreadModel>();

            int p = 0;

            while (unseenThreads.Count < unseenThreadsCount)
            {
                if (p > 0)
                {
                    if (inboxResponse.Inbox.HasOlder)
                    {
                        inboxResponse = user.Do(() => user.Direct.Inbox(inboxResponse.Inbox.OldestCursor, "older"));
                    }
                    else break;
                }

                var unseenThreadsFromResponse = inboxResponse
                    .Inbox
                    .Threads
                    .Where(t => t.IsUnseenThread(user.AccountId));

                unseenThreads.AddRange(unseenThreadsFromResponse);

                p++;
            }

            return unseenThreads;
        }

        private static void ShowInboxState(Instagram.Instagram user, InboxResponse inboxResponse)
        {
            user.Log(inboxResponse.PendingRequestsTotal > 0
                ? $"Direct pending requests total: {inboxResponse.PendingRequestsTotal}"
                : "No direct pending requests.");

            user.Log(inboxResponse.Inbox?.UnseenCount > 0
                ? $"Direct unseen threads total: {inboxResponse.Inbox.UnseenCount}"
                : "No direct unseen threads.");
        }

        private static List<ThreadModel> GetPendingThreads(Instagram.Instagram user)
        {
            var inboxResponse = user.Do(() => user.Direct.PendingInbox());

            List<ThreadModel> pendingThreads = new List<ThreadModel>();

            if (inboxResponse.Inbox.Threads.Any())
                pendingThreads.AddRange(inboxResponse.Inbox.Threads);

            while (inboxResponse.Inbox.HasOlder)
            {
                Utils.RandomSleep(2000, 5000);

                inboxResponse = user.Do(() => user.Direct.PendingInbox(inboxResponse.Inbox.OldestCursor, "older"));

                if (inboxResponse.Inbox.Threads.Any())
                    pendingThreads.AddRange(inboxResponse.Inbox.Threads);
            }

            return pendingThreads;
        }

        private static ThreadModel OpenThread(Instagram.Instagram user, ThreadModel thread)
        {
            var threadResponse = user.Do(() => user.Direct.OpenThread(thread.ThreadId));

            // Refresh thread data
            thread = threadResponse.Thread;

            // Accept if need it
            if (thread.Pending)
            {
                if (BlackListContains(thread.Username))
                {
                    user.Log($"Decline {thread.Username} request.");
                    user.Do(() => user.Direct.DeclineRequest(thread.ThreadId));
                    throw new DirectDeclineRequestException();
                }

                user.Log($"Accept {thread.Username} request.");
                user.Do(() => user.Direct.AcceptRequest(thread.ThreadId));
                AddUsernameToBlackList(thread.Username);
            }

            // todo: check oldest messages
            // todo: load all unseen messages

            return thread;
        }

        private static DirectThread InitThread(Instagram.Instagram user, ThreadModel thread)
        {
            DirectThread directThread;

            if (user.DirectRepository.AnyThreadByThreadId(thread.ThreadId))
            {
                directThread = user.DirectRepository.Init(thread.ThreadId);
            }
            else
            {
                directThread = new DirectThread
                {
                    LastActivityAt = thread.LastActivityAt,
                    ThreadId = thread.ThreadId,
                    Username = thread.Users.FirstOrDefault()?.Username,
                    Messages = new List<DirectThreadItem>()
                };

                user.DirectRepository.Create(directThread);
            }

            return directThread;
        }

        private static List<ThreadItemModel> SyncThread(Instagram.Instagram user, DirectThread directThread, ThreadModel thread)
        {
            var recentMessages = thread.Items
                .ConvertToDirectThreadItems()
                .OrderBy(m => m.Timestamp)
                .ToList();

            var unseenMessages = directThread.Messages.Any()
                ? recentMessages
                    .Where(m => m.Timestamp > directThread.LastActivityAt).ToList()
                : recentMessages;

            var unreedMessages = unseenMessages.Where(m => m.UserId != user.AccountId).ToList();

            user.Log($"New {unreedMessages.Count} messages in thread.");

            var lastOwnMessage = unseenMessages.LastOrDefault(m => m.UserId == user.AccountId);
            if (lastOwnMessage != null)
            {
                unreedMessages = unseenMessages.Where(m => m.Timestamp > lastOwnMessage.Timestamp).ToList();
            }

            if (unseenMessages.Any())
            {
                directThread.Messages
                    .AddRange(unseenMessages);
            }

            if (directThread.Messages.Any())
                directThread.LastActivityAt = directThread.Messages.LastOrDefault().Timestamp;

            user.DirectRepository.Update(directThread);

            return thread.Items
                .Where(t => unreedMessages
                    .Any(d => d.ItemId == t.ItemId))
                .ToList();

            //return unreedMessages;
        }

        private static Photo GetPhoto()
        {
            lock (PhotoLocker)
            {
                var photos =
                Directory.GetFiles(Path.Combine(Variables.DirectFolder, Variables.DirectPhotos))
                    .Where(
                        fileImage =>
                            Path.GetExtension(fileImage) == ".jpg" || Path.GetExtension(fileImage) == ".png" ||
                            Path.GetExtension(fileImage) == ".jpeg")
                    .ToList();

                string path = photos[Utils.Random.Next(0, photos.Count - 1)];

                return new Photo(path, isProfilePhoto: false, isDirectPhoto: true);
            }
        }

        private static void AddUsernameToBlackList(string username)
        {
            lock (BlackListLocker)
            {
                if (!LocalUsernamesBlackList.Contains(username))
                {
                    LocalUsernamesBlackList.Add(username);
                }
            }
        }

        private static bool BlackListContains(string username)
        {
            lock (BlackListLocker)
            {
                return LocalUsernamesBlackList.Contains(username);
            }
        }

        public static int GetDirectWay()
        {
            lock (_directWayLock)
            {
                if (_directWayCounter >= _directWays.Count)
                    _directWayCounter = 0;

                int directWay = _directWays[_directWayCounter];
                _directWayCounter++;

                return directWay;
            }
        }
    }
}
