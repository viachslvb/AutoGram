using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Database;
using Database.DirectSender;
using Database.Model;
using AutoGram.Helpers;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response.Direct;
using AutoGram.Instagram.Response.Friendship;
using AutoGram.Instagram.Response.Model;
using AutoGram.Services;
using AutoGram.Task.SubTask;
using Newtonsoft.Json;
using Org.BouncyCastle.Cms;
using xNet;
using static System.Net.Mime.MediaTypeNames;

namespace AutoGram.Task
{
    static class DirectSender
    {
        private static readonly List<DirectSenderProfileSource> SourceList;
        private static readonly List<DirectDomain> DomainsList;
        private static readonly List<DirectSenderTemplate> TemplateList = new List<DirectSenderTemplate>
        {
        };

        private static readonly List<string> PopularWomanNames = new List<string>
        {
            // some data
        };

        private static readonly List<string> GreetingsList = new List<string>
        {
            // some data
        };

        private static readonly List<string> SmilesGrettingsList = new List<string>
        {
            // some data
        };

        private static int _sourceCounter;
        private static int _messageCounter = 0;
        private static int _isSpamCounter = 0;
        private static int _isFailedCounter = 0;
        private static int _templateCounter = 0;
        private static int _domainCounter = 0;
        private static int _updateCounter = 0;

        private static readonly object OwnLocker = new object();
        private static readonly object CAListLocker = new object();
        private static readonly object DatabaseLocker = new object();
        private static readonly object DomainLocker = new object();
        private static readonly object TemplateLocker = new object();
        private static readonly object DataLocker = new object();
        private static readonly object UpdateSearchResultsLocker = new object();
        private static readonly object SaveDirectStatsLocker = new object();
        private static readonly object SyncBlackListLocker = new object();

        private static readonly object BlackListLocker = new object();
        private static readonly object FileWriterLocker = new object();

        // UI Update stats
        private static readonly object UpdateStatsLock = new object();

        // Database 
        private static readonly HashSet<UserDirect> DirectBlackList;
        public static readonly HashSet<UserDirect> DirectUsersUpdateList = new HashSet<UserDirect>();
        private static readonly HashSet<UserDirect> LocalRejectedList = new HashSet<UserDirect>();
        private static readonly HashSet<UserDirect> CancelledUserList = new HashSet<UserDirect>();

        static DirectSender()
        {
            var usernameSource = Settings.Advanced.DirectSender.UsernameSource;
            SourceList = usernameSource.Split(' ').Distinct().Select(x => new DirectSenderProfileSource { UserPk = x, IsLimitedView = false }).ToList();
            SourceList.Shuffle();
            TemplateList.Shuffle();

            var domainsData = Settings.Advanced.DirectSender.Domains;
            DomainsList = domainsData.Split(';').Select(x => new DirectDomain { Id = x.Split('|')[1], Url = x.Split('|')[0] }).ToList();

            DirectBlackList = new HashSet<UserDirect>(UsersDirectBlacklistRepository.GetAll());
        }

        public static void Do(Instagram.Instagram user, int countParticipants)
        {
            bool minimizeRequests = AutoGram.Settings.Advanced.General.MinimizeInstagramRequests;

            if (!minimizeRequests)
            {
                user.State.IgNavChain = "24F:explore_popular:2";
                user.Do(() => user.Internal.FbRecentSearches());
                user.Do(() => user.Internal.CommerceDestination());
                user.Do(() => user.Internal.FbSearchNullStateDynamicSections());
                user.Do(() => user.Discover.TopicalExplore());
                Utils.RandomSleep(4000, 6000);
            }

            var recipientsQueue = new Queue<UserDirectData>();
            var recipientsList = new List<UserDirect>();

            string source = string.Empty;

            while (true)
            {
                recipientsQueue.Clear();
                recipientsList.Clear();

                DirectSenderProfileSource profileSource;

                bool isMultipleSource = false;
                while (true)
                {
                    try
                    {
                        profileSource = user.Worker.IsSettings && user.Worker.Direct.Enable
                                        ? user.Worker.GetProfileSource()
                                        : GetProfileSource();
                    }
                    catch (EmptySourceException)
                    {
                        user.Log("Direct sender source is empty.");
                        throw new SuspendThreadWorkException();
                    }

                    if (profileSource.Disabled)
                        continue;

                    // Filter profile sources
                    if (profileSource.Muted)
                    {
                        profileSource.CurrentMuteCounter++;

                        if (profileSource.CurrentMuteCounter >= profileSource.MuteLimit)
                        {
                            profileSource.Muted = false;
                            profileSource.CurrentMuteCounter = 0;
                            profileSource.EndMuteTime = DateTime.Now;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var targetUser = new User { Pk = profileSource.UserPk };

                    Profile.ProfileResponse profileResponse;

                    try
                    {
                        profileResponse = Profile.Open(user, targetUser);

                        if (profileResponse.UserInfo.User.IsPrivate)
                        {
                            throw new LoadingFollowersException();
                        }
                    }
                    catch (OpenProfileException)
                    {
                        continue;
                    }
                    catch (ArgumentNullException ex)
                    {
                        user.Log(ex.Message);
                        continue;
                    }
                    catch (LoadingFollowersException)
                    {
                        user.Log($"User pk {targetUser.Pk} is private.");

                        lock (DataLocker)
                        {
                            if (user.Worker.IsSettings && user.Worker.Direct.Enable)
                            {
                                var tempSource = user.Worker.DirectSourceList.FirstOrDefault(x => x.UserPk == targetUser.Pk);
                                user.Worker.DirectSourceList.Remove(tempSource);
                            }
                            else
                            {
                                var tempSource = SourceList.FirstOrDefault(x => x.UserPk == targetUser.Pk);
                                SourceList.Remove(tempSource);
                            }
                        }

                        continue;
                    }
                    catch (UserNotFoundException)
                    {
                        user.Log($"User pk {targetUser.Pk} not found.");

                        lock (DataLocker)
                        {
                            if (user.Worker.IsSettings && user.Worker.Direct.Enable)
                            {
                                var tempSource = user.Worker.DirectSourceList.FirstOrDefault(x => x.UserPk == targetUser.Pk);
                                user.Worker.DirectSourceList.Remove(tempSource);
                            }
                            else
                            {
                                var tempSource = SourceList.FirstOrDefault(x => x.UserPk == targetUser.Pk);
                                SourceList.Remove(tempSource);
                            }
                        }

                        continue;
                    }

                    var targetUsername = profileResponse.UserInfo.User.Username;

                    user.Log($"Opening profile {targetUsername}");

                    string nextMaxId = string.Empty;
                    string rankToken = Utils.GenerateUUID(true);

                    bool isEnough = false;

                    int deepFollowersLoad = 0;
                    int allowedParticipants = 0;
                    int allNewUsersFromSource = 0;

                    FriendshipsResponse friendshipsResponse = new FriendshipsResponse();

                    // Get users
                    bool isMediaChecked = false;
                    bool followersChecked = false;
                    bool goToMedia = false;
                    bool isMediaLikers = false;

                    while (true)
                    {
                        var usersFiltered = new List<UserDirect>();

                        if (Settings.Advanced.DirectSender.UseLikers && !isMediaChecked || goToMedia && !isMediaChecked)
                        {
                            if (profileResponse.UserFeed != null && profileResponse.UserFeed.MediaItems.Any())
                            {
                                var mediaItem = profileResponse.UserFeed.MediaItems.FirstOrDefault();
                                var last30Hours = Utils.DateTimeNowTotalSeconds - 108000;

                                if (mediaItem.TakenAt > last30Hours)
                                {
                                    var postedAgo = (Utils.DateTimeNowTotalSeconds - mediaItem.TakenAt) / 3600;

                                    friendshipsResponse = user.Do(() => user.Media.GetLikers(mediaItem.Id));
                                    isMediaLikers = true;
                                }
                                else
                                {
                                    isMediaChecked = true;
                                    continue;
                                }
                            }
                            else
                            {
                                isMediaChecked = true;
                                continue;
                            }
                        }
                        else
                        {
                            if (followersChecked) break;

                            if (user.Worker.IsSettings && user.Worker.Settings.Direct.Enable && user.Worker.Settings.Direct.TurnOnLikesSourceOnly)
                            {
                                if (profileResponse.UserFeed != null && profileResponse.UserFeed.MediaItems.Any())
                                {
                                    bool notFound = true;

                                    for (var mi = 0; mi < 3; mi++)
                                    {
                                        var mediaItem = profileResponse.UserFeed.MediaItems[mi];

                                        if (user.Worker.Settings.Direct.UseSourceItemsAcceptedOnly)
                                        {
                                            var acceptedLikesSourceItems = new List<string>();
                                            acceptedLikesSourceItems = user.Worker.Settings.Direct.LikesSourceItemsAccepted.Split(' ').ToList();

                                            if (!acceptedLikesSourceItems.Contains(mediaItem.Code))
                                            {
                                                profileSource.Disabled = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var last30Hours = Utils.DateTimeNowTotalSeconds - 324000;

                                            if (mediaItem.TakenAt < last30Hours)
                                            {
                                                break;
                                            }
                                        }

                                        user.Log($"Open photo http://www.instagram.com/p/{mediaItem.Code}/");
                                        friendshipsResponse = user.Do(() => user.Media.GetLikers(mediaItem.Id));

                                        usersFiltered.AddRange(
                                            friendshipsResponse
                                                .Users
                                                .ConvertToUserDirects()
                                                .Where(u => u.IsAccepted(Settings.Advanced.DirectSender.Filtration))
                                                .ToList()
                                        );

                                        notFound = true;
                                    }

                                    if (!notFound) break;

                                    isMediaLikers = true;
                                    user.PigeonSessionId = Utils.GeneratePigeonSession();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                // Load followers
                                friendshipsResponse = user.Do(() =>
                                    user.FriendShips.GetFollowers(profileResponse.UserInfo.User.Pk, rankToken, nextMaxId));

                                // Filtration
                                usersFiltered = friendshipsResponse
                                    .Users
                                    .ConvertToUserDirects();

                                if (Settings.Advanced.DirectSender.Filtration.Enable)
                                {
                                    usersFiltered = usersFiltered
                                        .Where(u => u.IsAccepted(Settings.Advanced.DirectSender.Filtration))
                                        .ToList();
                                }

                            }
                        }

                        if (Settings.Advanced.DirectSender.Filtration.Enable)
                        {
                            if (Settings.Advanced.DirectSender.Filtration.UseWhiteList)
                            {
                                usersFiltered = usersFiltered
                                    .Where(u => u.IsContainsInWhiteList(Settings.Advanced.DirectSender.Filtration.UseSurnameWhiteList, isMediaLikers))
                                    .ToList();
                            }
                            else if (Settings.Advanced.DirectSender.Filtration.UseReverseWhiteList)
                            {
                                usersFiltered = usersFiltered
                                    .Where(u => !u.IsContainsInWhiteList())
                                    .ToList();
                            }
                        }

                        int founded = 0;
                        int newUsersCounter = 0;

                        lock (DatabaseLocker)
                        {
                            var noRejectedUsers = usersFiltered.Where(u => LocalRejectedList.All(a => a.Pk != u.Pk)).ToList();

                            if (noRejectedUsers.Any())
                            {
                                var acceptedUsers = noRejectedUsers.Where(u => DirectBlackList.All(b => b.Pk != u.Pk)).ToList();

                                // Local rejected users database
                                var rejectedUsers = noRejectedUsers.Where(u => acceptedUsers.All(a => a.Pk != u.Pk)).ToList();

                                foreach (var rejectedUser in rejectedUsers)
                                {
                                    LocalRejectedList.Add(rejectedUser);
                                }

                                newUsersCounter = acceptedUsers.Count;

                                allNewUsersFromSource += acceptedUsers.Count;

                                var takedUsers = new List<UserDirect>();

                                foreach (var acceptedUser in acceptedUsers)
                                {
                                    takedUsers.Add(acceptedUser);
                                    recipientsQueue.Enqueue(new UserDirectData
                                    {
                                        Pk = acceptedUser.Pk,
                                        Username = acceptedUser.Username,
                                        FullName = acceptedUser.FullName,
                                        IsPrivate = acceptedUser.IsPrivate,
                                        SourcePk = targetUser.Pk,
                                        SourceUsername = targetUsername
                                    });
                                    recipientsList.Add(acceptedUser);
                                    allowedParticipants++;
                                    founded++;

                                    // Add to blacklist
                                    DirectBlackList.Add(acceptedUser);
                                    DirectUsersUpdateList.Add(acceptedUser);

                                    if (recipientsQueue.Count >= countParticipants) // Settings.Advanced.DirectSender.ParticipantsCount
                                    {
                                        isEnough = true;
                                        break;
                                    }
                                }
                            }
                        }

                        var typeUsers = isMediaLikers ? "likers" : "followers";

                        user.Log($"{usersFiltered.Count} are accepted by filtration, new users {newUsersCounter}, taken {founded}. Already {recipientsQueue.Count}");

                        if (isEnough) break;

                        if (!Settings.Advanced.DirectSender.UseLikers)
                        {
                            deepFollowersLoad++;

                            if (friendshipsResponse != null && friendshipsResponse.IsNextMaxId)
                            {
                                nextMaxId = friendshipsResponse.NextMaxId;
                            }
                            else break;

                            if (deepFollowersLoad >= Settings.Advanced.DirectSender.DeepParsing)
                            {
                                if (Settings.Advanced.DirectSender.UseLikersWhenFollowersNotEnough && !isMediaChecked)
                                {
                                    followersChecked = true;
                                    goToMedia = true;
                                    continue;
                                }

                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (allowedParticipants >= 5)
                    {
                        source = profileSource.UserPk;
                    }

                    if (Settings.Advanced.DirectSender.MuteLevelsIsEnabled)
                    {
                        if (allNewUsersFromSource <= 2)
                        {
                            if (string.IsNullOrEmpty(profileSource.Username)) profileSource.Username = targetUsername;

                            profileSource.Muted = true;

                            if (profileSource.MuteLevel == 1)
                            {
                                profileSource.MuteLimit = 15;
                            }
                            else if (profileSource.MuteLevel == 2)
                            {
                                profileSource.MuteLimit = 25;
                            }
                            else if (profileSource.MuteLevel == 3)
                            {
                                profileSource.MuteLimit = 35;
                            }
                            else if (profileSource.MuteLevel == 4)
                            {
                                profileSource.MuteLimit = 45;
                            }
                            else if (profileSource.MuteLevel >= 5)
                            {
                                profileSource.MuteLimit = 60;

                                lock (FileWriterLocker)
                                {
                                    try
                                    {
                                        using (TextWriter textWriter = new StreamWriter("low_sources.txt", true))
                                        {
                                            textWriter.WriteLine(profileSource.UserPk);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //throw new Exception();
                                    }
                                }
                            }
                            else profileSource.MuteLimit = 10;

                            profileSource.BeginMuteTime = DateTime.Now;
                            profileSource.MuteLevel++;
                        }
                        else
                        {
                            if (profileSource.MuteLevel > 0)
                            {
                                profileSource.MuteLevel = 0;
                            }
                        }
                    }

                    if (isEnough) break;
                }

                if (source == string.Empty)
                    source = "Other";

                if (Settings.Advanced.DirectSender.ParseOnlyMode)
                {
                    lock (SyncBlackListLocker)
                    {
                        if (DirectUsersUpdateList.Any())
                            UsersDirectBlacklistRepository.AddRange(DirectUsersUpdateList.ToList());

                        DirectUsersUpdateList.Clear();
                    }

                    lock(CAListLocker)
                    {
                        try
                        {
                            using (TextWriter textWriter = new StreamWriter("calist.txt", true))
                            {
                                foreach (var recipient in recipientsList)
                                {
                                    textWriter.WriteLine(recipient.Pk);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //throw new Exception();
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (!Settings.Advanced.DirectSender.SendOneMessage)
            {
                var directDomain = GetDirectDomain();
                var directUrl = directDomain.Url;

                if (user.Worker.IsSettings && user.Worker.Direct.Enable && user.Worker.Direct.UseCustomDomain)
                {
                    directDomain = new DirectDomain { Id = user.Worker.Direct.Domain.Split('|')[1], Url = user.Worker.Direct.Domain.Split('|')[0] };
                    directUrl = directDomain.Url;
                }

                string link = string.Empty;
                string linkText = string.Empty;
                string titleText = Settings.Advanced.DirectSender.Title;

                // Create conversation
                var usersForCreating = new List<string>();
                if (Settings.Advanced.DirectSender.AddAllToConversation)
                {
                    usersForCreating.AddRange(recipientsQueue.ToList().Select(x => x.Pk).ToList());
                }

                user.State.IgNavChain = "24F:explore_popular:2,4tf:direct_thread:3";

                try
                {
                    var messageText = Settings.Advanced.DirectSender.MessageText.Replace("{link}", link);

                    var createThread = !Settings.Advanced.DirectSender.SendMessageAsText
                        ? user.Do(() =>
                    user.Direct.SendLinkToRecipientUsers(link, link,
                        usersForCreating.EncodeList(false)))
                        : user.Do(() =>
                    user.Direct.SendTextToRecipientUsers(messageText,
                        usersForCreating.EncodeList(false)));

                    if (createThread.IsOk())
                    {
                        string threadId = createThread.Payload.ThreadId;

                        user.Log($"Create conversation {threadId}");

                        // Open direct thread
                        user.Log($"Opening thread");
                        var threadResponse = user.Do(() => user.Direct.OpenThread(threadId));

                        if (Settings.Advanced.DirectSender.SendAdditionalMessageText)
                        {
                            var additionalMessageText = Settings.Advanced.DirectSender.AdditionalMessageText;

                            var additionalMessageSendingResponse = user.Do(() => user.Direct.SendText(additionalMessageText, threadId));

                            if (additionalMessageSendingResponse.IsOk())
                            {
                                user.Log($"Sent additional message.");
                            }
                            else user.Log($"Failed sent additional message.");
                        }

                        if (Settings.Advanced.DirectSender.AddUsersToConversation)
                        {
                            // Add participants to thread
                            var participantsList = new List<UserDirect>(recipientsQueue);

                            var usersForAdding = participantsList.Select(u => u.Pk).ToList();
                            var usersForAddingQueue = new Queue<string>(usersForAdding);

                            while (usersForAddingQueue.Any())
                            {
                                var listUserA = new List<string>();

                                if (usersForAddingQueue.Any())
                                    listUserA.Add(usersForAddingQueue.Dequeue());

                                var addingUsersResponse = user.Do(() => user.Direct.AddParticipantsToThread(threadId, listUserA.EncodeList()));

                                if (addingUsersResponse.IsOk())
                                {
                                    user.Log($"Adding {listUserA.Count} users to thread {threadId}");
                                }
                                else
                                {
                                    user.Log($"Failed adding users: {addingUsersResponse.Message}");
                                    Log.Write(addingUsersResponse.HttpResponse.ToString());
                                }

                                user.Log($"Sleep 500ms");
                                Thread.Sleep(500);
                            }

                            if (Settings.Advanced.DirectSender.UpdateTitle)
                            {
                                Utils.RandomSleep(2000, 3000);

                                // Update title
                                var updateTitleResponse = user.Do(() => user.Direct.UpdateDirectThreadTitle(threadId, titleText));

                                if (updateTitleResponse.IsOk())
                                {
                                    user.Log("Title updated.");
                                }
                                else
                                {
                                    // Update title
                                    updateTitleResponse = user.Do(() => user.Direct.UpdateDirectThreadTitle(threadId, titleText));

                                    if (updateTitleResponse.IsOk())
                                    {
                                        user.Log("Title updated.");
                                    }
                                }
                            }

                            lock (UpdateStatsLock)
                            {
                                Worker.DirectSuccess++;
                                Worker.UpdateDirectStats();
                            }

                            lock (OwnLocker)
                            {
                                _messageCounter++;
                                _isFailedCounter = 0;
                            }

                            // If the conversation creation failed for more than X, then suspend work thread
                            lock (OwnLocker)
                            {
                                _isFailedCounter++;

                                if (_isFailedCounter > 25)
                                    throw new SuspendThreadWorkException("Conversation creation is failed more than 25 times.");
                            }

                            user.Storage.IsDirectSenderFeedback = true;
                            user.Storage.Save();

                            lock (UpdateStatsLock)
                            {
                                Worker.DirectFailed++;
                                Worker.UpdateDirectStats();

                                using (TextWriter textWriter = new StreamWriter("accounts_failed_adding_checkpoint.txt", true))
                                {
                                    textWriter.WriteLine($"{user.Username}:{user.Password}::");
                                }
                            }

                            throw new SuspendTaskException();
                        }
                        else
                        {
                            lock (UpdateStatsLock)
                            {
                                Worker.DirectSuccess++;
                                Worker.UpdateDirectStats();
                            }

                            lock (OwnLocker)
                            {
                                _messageCounter++;
                                _isFailedCounter = 0;
                            }

                            if (Settings.Advanced.DirectSender.UpdateTitle)
                            {
                                string name = user.Username.Split('.')[0].FirstCharToUpper();

                                // Update title
                                var updateTitleResponse = user.Do(() => user.Direct.UpdateDirectThreadTitle(threadId, titleText));

                                if (updateTitleResponse.IsOk())
                                {
                                    user.Log("Title updated.");
                                }
                            }
                        }

                        return;
                        //throw new SuspendTaskException();
                    }
                    else
                    {
                        user.Log($"Failed create conversation: {createThread.Message}");

                        lock (UpdateStatsLock)
                        {
                            using (TextWriter textWriter = new StreamWriter("accounts_failed_creating_checkpoint.txt", true))
                            {
                                textWriter.WriteLine($"{user.Username}:{user.Password}::");
                            }
                        }
                    }
                }
                catch (LoginRequiredException)
                {
                    lock (SyncBlackListLocker)
                    {
                        var userForDeleting =
                            new List<UserDirect>(recipientsList);

                        foreach (var userDirect in userForDeleting)
                        {
                            CancelledUserList.Add(userDirect);
                        }
                    }

                    throw new LoginRequiredException();
                }

                lock (SyncBlackListLocker)
                {
                    var userForDeleting =
                        new List<UserDirect>(recipientsList);

                    foreach (var userDirect in userForDeleting)
                    {
                        CancelledUserList.Add(userDirect);
                    }
                }

                lock (UpdateStatsLock)
                {
                    Worker.DirectFailed++;
                    Worker.UpdateDirectStats();
                }

                user.Storage.IsDirectSenderFeedback = true;
                user.Storage.Save();

                // If the conversation creation failed for more than X, then suspend work thread
                lock (OwnLocker)
                {
                    _isFailedCounter++;

                    if (_isFailedCounter > 25)
                        throw new SuspendThreadWorkException("Conversation creation is failed more than 25 times.");
                }

                throw new SuspendTaskException();
            }
            else
            {
                var usersToSaveInDB = new List<DirectStatsModel>();
                string messageText = Settings.Advanced.DirectSender.MessageText;
                List<Thread> createThreadThreads = new List<Thread>();
                var createThreadLocker = new object();


                try
                {
                    while (recipientsQueue.Any())
                    {
                        var directTemplate = GetDirectTemplate();
                        string domain = "somedata";
                        string link = "somedata";

                        var recipientUserDirect = recipientsQueue.Dequeue();

                        var recipientList = new List<string>() { recipientUserDirect.Pk };
                        var createThreadParameters = ((Instagram.Instagram)user.Clone(), recipientList, link);

                        Thread createThreadOperation = new Thread(delegate (object param)
                        {
                            var createThreadStatus = user.Do(() => SendToRecipientMessage(param));

                            if (createThreadStatus.IsOk())
                            {
                                user.Log($"Send message to {recipientUserDirect.Username} successfully.");

                                lock (usersToSaveInDB)
                                {
                                    var userDirectInfo = new DirectStatsModel
                                    {
                                        CreatedAt = DateTime.Now,
                                        From = user.Username,
                                        Recipient = recipientUserDirect.Username,
                                        SourcePk = recipientUserDirect.SourcePk,
                                        SourceUsername = recipientUserDirect.SourceUsername
                                    };

                                    usersToSaveInDB.Add(userDirectInfo);
                                }

                                lock (UpdateStatsLock)
                                {
                                    Worker.DirectSuccess++;
                                    Worker.UpdateDirectStats();
                                }
                            }
                            else
                            {
                                user.Log($"Failed create conversation with {recipientUserDirect.Username}");
                                //MessageBox.Show("Failed");
                                lock (UpdateStatsLock)
                                {
                                    Worker.DirectFailed++;
                                    Worker.UpdateDirectStats();
                                }

                                lock (SyncBlackListLocker)
                                {
                                    var userForDeleting =
                                        new List<UserDirect>(recipientsQueue) { recipientUserDirect };

                                    foreach (var userDirect in userForDeleting)
                                    {
                                        CancelledUserList.Add(userDirect);
                                    }
                                }

                                //throw new SuspendTaskException();
                            }

                        });

                        createThreadOperation.Start(createThreadParameters);
                        createThreadThreads.Add(createThreadOperation);
                    }

                    foreach (var thread in createThreadThreads)
                    {
                        thread.Join();
                    }

                    lock (SaveDirectStatsLocker)
                    {
                        using (var ctx = new DirectStatsContext())
                        {
                            ctx.DirectStats.AddRangeAsync(usersToSaveInDB);
                            ctx.SaveChangesAsync();
                        }
                    }
                }
                catch (Http403Exception e)
                {
                    user.Log("Sending error 403.");
                    throw new SuspendTaskException();
                }
                finally
                {
                    lock (SyncBlackListLocker)
                    {
                        foreach (var cancelledUser in CancelledUserList)
                        {
                            if (DirectBlackList.Any(x => x.Pk == cancelledUser.Pk))
                            {
                                DirectBlackList.RemoveWhere(x => x.Pk == cancelledUser.Pk);
                            }

                            if (DirectUsersUpdateList.Any(x => x.Pk == cancelledUser.Pk))
                            {
                                DirectUsersUpdateList.RemoveWhere(x => x.Pk == cancelledUser.Pk);
                            }
                        }

                        CancelledUserList.Clear();

                        if (DirectUsersUpdateList.Any())
                            UsersDirectBlacklistRepository.AddRange(DirectUsersUpdateList.ToList());

                        DirectUsersUpdateList.Clear();
                    }
                }

                user.Log("All messages was sent.");
                throw new SuspendTaskException();
            }
        }

        private static SendMessageResponse SendToRecipientMessage(object parameters)
        {
            var (userObject, recipientsUserIdList, link) = ((Instagram.Instagram, List<string>, string))parameters;

            var messages = Settings.Advanced.DirectSender.Messages.ToList();
            var message = messages[Utils.Random.Next(0, messages.Count)];

            Instagram.Request.Request request = new Instagram.Request.Request
            {
                AllowAutoRedirect = true,
                IgnoreProtocolErrors = true,
                Reconnect = true,
                ReconnectLimit = 5,
                ReadWriteTimeout = 40000,
                ConnectTimeout = 40000,
                Cookies = new CookieDictionary(),
                KeepAlive = true
            };

            request.SetApp(userObject.App);
            request.SetDevice(userObject.Device);
            request.SetUser(userObject);
            request.UserAgent = userObject.GetUserAgent();
            request.Proxy = userObject.Request.Proxy;
            request.Cookies = userObject.GetCookies();

            if (Settings.Advanced.DirectSender.SendMessageAsText)
            {
                // Send As Message

                return request
                    .AddDefaultHeaders()
                    .AddParam("recipient_users", $"[[{recipientsUserIdList.EncodeList(false)}]]")
                    .AddParam("action", "send_item")
                    .AddParam("client_context", Utils.GenerateUUID(true))
                    .AddParam("_csrftoken", userObject.GetToken())
                    .AddParam("text", message)
                    .AddParam("_uuid", userObject.Uuid)
                    .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/text/")
                    .ToResponse<SendMessageResponse>()
                    ;
            }
            else
            {
                // Send Link

                string clientContext = Utils.GenerateClientContextToken();

                return request
                    .AddDefaultHeaders()
                    .AddParam("recipient_users", $"[[{recipientsUserIdList.EncodeList(false)}]]")
                    .AddParam("link_text", link ?? string.Empty)
                    .AddParam("link_urls", $"[{new[] { link }.EncodeList()}]")
                    .AddParam("action", "send_item")
                    .AddParam("is_shh_mode", "0")
                    .AddParam("send_attribution", "inbox_new_message")
                    .AddParam("client_context", clientContext)
                    .AddParam("device_id", userObject.DeviceId)
                    .AddParam("mutation_token", clientContext)
                    .AddParam("_uuid", userObject.Uuid)
                    .AddParam("nav_chain", userObject.State.IgNavChain)
                    .AddParam("offline_threading_id", clientContext)
                    .Post("https://i.instagram.com/api/v1/direct_v2/threads/broadcast/link/")
                    .ToResponse<SendMessageResponse>();
            }
        }

        private static DirectSenderProfileSource GetProfileSource()
        {
            lock (DataLocker)
            {
                if (SourceList.Count < 1)
                {
                    throw new EmptySourceException();
                }

                if (SourceList.Count > _sourceCounter)
                    return SourceList[_sourceCounter++];

                _sourceCounter = 0;
                return SourceList[_sourceCounter++];
            }
        }

        private static DirectSenderTemplate GetDirectTemplate()
        {
            lock (TemplateLocker)
            {
                if (TemplateList.Count > _templateCounter)
                    return TemplateList[_templateCounter++];

                _templateCounter = 0;
                return TemplateList[_templateCounter++];
            }
        }

        private static DirectDomain GetDirectDomain()
        {
            lock (DomainLocker)
            {
                if (DomainsList.Count > _domainCounter)
                    return DomainsList[_domainCounter++];

                _domainCounter = 0;
                return DomainsList[_domainCounter++];
            }
        }
    }

    public class DirectSenderTemplate
    {
        public int Id { get; set; }
        public string ConversationTitle { get; set; }
        public string ConversationGreetings { get; set; }
    }

    public class DirectTargetUserData
    {
        public string Username;
        public string Fullname;
        public string Pk;
        public string ProfilePictureUrl;
    }

    public class DirectDomain
    {
        public string Id;
        public string Url;
    }

    public class DirectSenderProfileSource
    {
        public string UserPk { get; set; }
        public string Username { get; set; }
        public bool Muted { get; set; }
        public int MuteLevel { get; set; }
        public int MuteLimit { get; set; }
        public int CurrentMuteCounter { get; set; }
        public DateTime BeginMuteTime { get; set; }
        public DateTime EndMuteTime { get; set; }
        public bool IsLimitedView { get; set; }
        public bool Disabled { get; set; }
    }
}
