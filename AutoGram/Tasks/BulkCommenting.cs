using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AutoGram.Helpers;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;
using AutoGram.Task.SubTask;

namespace AutoGram.Task
{
    static class BulkCommenting
    {
        private static readonly List<string> CommentsList;
        private static readonly List<string> TagList;
        private static readonly HashSet<string> PublishedCommentsList = new HashSet<string>();

        private static int _commentCounter;
        private static int _tagCounter;

        private static readonly object DataLocker = new object();
        private static readonly object WriteCommentsListLocker = new object();

        static BulkCommenting()
        {
            // Tags source
            var tagSource = Settings.Advanced.Comment.TagSource;
            var tags = (from t in tagSource.Split(' ') select t.Replace("#", "")).ToList();
            tags = tags.Distinct().ToList();
            tags.Shuffle();
            TagList = new List<string>(tags);

            // Comment source
            var commentsSource = Settings.Advanced.Comment.CommentSource;
            CommentsList = commentsSource.Split('|').ToList();
        }

        public static void Do(Worker worker, Instagram.Instagram user)
        {
            int limit = Utils.Random.Next(Settings.Advanced.Comment.QuantityLimit.From,
                Settings.Advanced.Comment.QuantityLimit.To);
            int delayFrom = Settings.Advanced.Comment.Delay.From;
            int delayTo = Settings.Advanced.Comment.Delay.To;

            int num = 0;
            int errors = 0;
            int errorLimit = 5;

            // Open discover explore

            user.Log($"Open discover explore.");
            user.Do(() => user.Discover.Explore());
            Utils.RandomSleep(3000, 7000);

            // todo: fix
            user.Activity.Sent.Total = 0;

            int emptyFeedError = 0;
            int tagNotFoundError = 0;

            while (true)
            {
                // Search for the hashtag we need

                var hashtag = GetTag();
                user.Log($"Search for the #{hashtag}.");
                var searchResponse = user.Do(() => user.Hashtag.Search(hashtag));
                Utils.RandomSleep(2000, 6000);

                #region Check Hashtag
                if (searchResponse.IsOk())
                {
                    if (!searchResponse.IsValidHashtag(hashtag))
                    {
                        if (tagNotFoundError > 2)
                        {
                            throw new SuspendTaskException();
                        }

                        user.Log($"Hashtag #{hashtag} not found.");
                        SaveLog($"Hashtag #{hashtag} not found.");
                        Log.Write($"Hashtag #{hashtag} not found.", LogResource.BulkCommenting);

                        tagNotFoundError++;
                        continue;
                    }

                    tagNotFoundError = 0;
                }
                else
                {
                    string errorMessage = searchResponse.IsMessage()
                        ? searchResponse.GetMessage()
                        : $"Error searching for hashtag {hashtag}";

                    user.Log(errorMessage);
                    Log.Write(errorMessage, LogResource.BulkCommenting);

                    continue;
                }
                #endregion



                // Open the hashtag

                user.Log($"Open #{hashtag} page.");
                user.Hashtag.OpenHashtagPage(hashtag);

                // Load feed

                int isTop = 0;
                var medias = GetFeedByHashtag(user, hashtag, ref isTop);

                if (!medias.Any())
                {
                    user.Log($"No media by #{hashtag}.");
                    continue;
                }

                var mediaList = (from media in medias from instaMediaAlbumResponse in media select instaMediaAlbumResponse.Media).ToList();

                var mediasIdList = new List<string>();

                int takeMediaCount = Settings.Advanced.Comment.TakeMediaCount;

                if (mediaList.Count > takeMediaCount)
                    mediaList = mediaList.Take(takeMediaCount).ToList();

                if (!Settings.Advanced.Comment.UseCleverSniffer)
                {
                    mediaList = mediaList
                        .Where(m => m.CommentCount >= Settings.Advanced.Comment.MediaWhereCommentCount.From
                                    && m.CommentCount <= Settings.Advanced.Comment.MediaWhereCommentCount.To)
                        .Where(m => m.LikeCount >= Settings.Advanced.Comment.MediaWhereLikeCount.From
                                    && m.LikeCount <= Settings.Advanced.Comment.MediaWhereLikeCount.To)
                        .ToList();
                }

                user.Log($"All media founded: {mediaList.Count}");

                if (Settings.Advanced.Comment.MediaOnlyForTheLastDays > 0)
                {
                    var minTakenAtDate = DateTime.UtcNow.ToUnixTime() - Settings.Advanced.Comment.MediaOnlyForTheLastDays * 86400;
                    mediaList = mediaList.Where(m => m.TakenAt > minTakenAtDate).ToList();

                    user.Log($"Media taken in the last {Settings.Advanced.Comment.MediaOnlyForTheLastDays} days: {mediaList.Count}");
                }

                if (Settings.Advanced.Comment.PreviewCommentsFilter.Enable)
                {
                    var minTakenAtDate = DateTime.UtcNow.ToUnixTime() - Settings.Advanced.Comment.PreviewCommentsFilter.MinutesLimit * 60;

                    mediaList = mediaList
                        .Where(m => m.PreviewComments != null && m.PreviewComments.Any())
                        .Where(m => m.PreviewComments.FirstOrDefault().CreatedAt > minTakenAtDate)
                        .ToList();

                    user.Log($"With comments in last {Settings.Advanced.Comment.PreviewCommentsFilter.MinutesLimit} minutes: {mediaList.Count}");

                    if (Settings.Advanced.Comment.PreviewCommentsFilter.DontRepeatIfCommentedAlready)
                    {
                        lock (WriteCommentsListLocker)
                        {
                            mediaList = mediaList
                                .Where(m => !PublishedCommentsList.Contains(m.Pk))
                                .ToList();
                        }

                        user.Log($"No commented yet: {mediaList.Count}");
                    }

                    if (Settings.Advanced.Comment.PreviewCommentsFilter.OrderByCreatedDate)
                    {
                        mediaList = mediaList
                           .OrderByDescending(m => m.PreviewComments.LastOrDefault().CreatedAt)
                           .ToList();
                    }
                }

                // Clever sniffer
                if (Settings.Advanced.Comment.UseCleverSniffer)
                {
                    var temporallyMediaList = new List<Instagram.Response.Model.MediaItem>();

                    foreach (var mediaItem in mediaList)
                    {
                        string mediaCaption = mediaItem.Caption?.Text;
                        if (string.IsNullOrEmpty(mediaCaption)) continue;

                        if (mediaCaption.IsEqualToPattern(
                            @"(?<=)([a-z]+\s#[a-z]+\s[a-z]+\s#[a-z]+\s[a-z]+\s#[a-z]+\s[a-z]+)"))
                        {
                            temporallyMediaList.Add(mediaItem);
                            continue;
                        }

                        if (mediaCaption.IsEqualToPattern(@"(?<=)(@[a-z0-9A-Z]+)"))
                        {
                            if (mediaItem.CommentCount >= Settings.Advanced.Comment.MediaWhereCommentCount.From
                                && mediaItem.CommentCount <= Settings.Advanced.Comment.MediaWhereCommentCount.To)
                            {
                                if (mediaItem.MediaType == 8 || mediaItem.MediaType == 2)
                                {
                                    temporallyMediaList.Add(mediaItem);
                                    continue;
                                }
                            }
                        }

                        if (mediaItem.MediaType == 8 || mediaItem.MediaType == 2)
                        {
                            if (mediaItem.CommentCount >= Settings.Advanced.Comment.MediaWhereCommentCount.From
                                && mediaItem.CommentCount <= Settings.Advanced.Comment.MediaWhereCommentCount.To)
                            {
                                temporallyMediaList.Add(mediaItem);
                                continue;
                            }
                        }

                        // Add another ... 

                        continue;
                    }

                    mediaList.Clear();
                    mediaList.AddRange(temporallyMediaList);
                }

                if (!mediaList.Any())
                {
                    if (emptyFeedError > 3)
                        throw new SuspendTaskException();

                    emptyFeedError++;
                    continue;
                }
                else emptyFeedError = 0;

                if (Settings.Advanced.Comment.ShuffleMedia)
                    mediaList.Shuffle();

                SaveLog($"Hashtag: #{hashtag} {isTop}", $"{Variables.FolderComments}{user.Username}.txt");

                foreach (var media in mediaList)
                {
                    // Like media

                    if (Utils.UseIt(4))
                    {
                        Like.Media(user, media.User, media.Id);
                    }

                    Utils.RandomSleep(3000, 6000);

                    user.Log($"Open comments https://instagram.com/p/{media.Code}/");
                    var commentsResponse = user.Do(() => user.Media.GetComments(media.Id));

                    // Filter commenting frequency
                    if (Settings.Advanced.Comment.RecentThreeCommentsFilter.Enable)
                    {
                        if (commentsResponse.Comments?.Count > 3)
                        {
                            var recentComments = commentsResponse.Comments.TakeLast(3);
                            var nowUnixTime = DateTime.UtcNow.ToUnixTime();

                            if (nowUnixTime - recentComments.FirstOrDefault().CreatedAt > Settings.Advanced.Comment.RecentThreeCommentsFilter.MinutesLimit * 60)
                            {
                                var lastCommentCreatedAt = (nowUnixTime - recentComments.FirstOrDefault().CreatedAt) / 60;

                                user.Log($"Last comment was published {lastCommentCreatedAt} minutes ago.");
                                continue;
                            }
                        }
                    }

                    if (Settings.Advanced.Comment.DontRepeatIfCommentedAlreadyInLast > 0)
                    {
                        var commentsCount = Settings.Advanced.Comment.DontRepeatIfCommentedAlreadyInLast;

                        var recentComments = commentsResponse.Comments?.Count > commentsCount
                            ? commentsResponse.Comments.TakeLast(commentsCount)
                            : commentsResponse.Comments;

                        lock (WriteCommentsListLocker)
                        {
                            bool isCommentedAlready = recentComments.Any(c => PublishedCommentsList.Contains(c.Pk));

                            if (isCommentedAlready)
                            {
                                user.Log("Already commented.");
                                continue;
                            }
                        }
                    }

                    // Add comment

                    string comment = GetComment();
                    var response = user.Do(() => user.Media.Comment(media.Id, comment));

                    if (response.IsOk())
                    {
                        user.Log($"Comment was succesfully published to https://instagram.com/p/{media.Code}/");
                        //worker.Account.UpdateSentCount(++num);

                        if (!response.IsComment())
                        {
                            user.Log("But a comment was deleted.");
                        }
                        else
                        {
                            lock (WriteCommentsListLocker)
                            {
                                PublishedCommentsList.Add(response.Comment.pk);
                            }

                            SaveLog($"https://instagram.com/p/{media.Code}/", $"{Variables.FolderComments}{user.Username}.txt");
                            SaveLog($"https://instagram.com/p/{media.Code}/", $"{Variables.FileCommentsLinkLog}");
                            errors = 0;
                        }

                        user.Activity.Sent.Total++;
                    }
                    else
                    {
                        string errorMessage = response.IsMessage()
                            ? response.GetMessage()
                            : "Error commenting";

                        user.Log(errorMessage);
                        Log.Write(errorMessage, LogResource.BulkCommenting);
                        errors++;
                    }

                    // Sleep

                    var sleepTime = Utils.Random.Next(delayFrom,
                    delayTo);
                    worker.Account.WriteLog($"Sleep {sleepTime}s.");
                    Thread.Sleep(sleepTime * 1000);

                    if (errors >= errorLimit)
                    {
                        user.Log($"Error limit.");
                        throw new SuspendTaskException();
                    }

                    if (user.Activity.Sent.Total >= limit)
                    {
                        throw new SuspendTaskException();
                    }
                }
            }
        }

        private static IEnumerable<List<MediaAlbumResponse>> GetFeedByHashtag(Instagram.Instagram user, string hashtag, ref int isTop)
        {
            string rankToken = Utils.GenerateUUID(true);

            HashtagMediaResponse hashtagResponse;
            if (Settings.Advanced.Comment.LoadPopularFeed
                && Settings.Advanced.Comment.LoadRecentFeed)
            {
                if (Utils.UseIt())
                {
                    user.Log($"Load popular feed.");
                    isTop = 1;
                    hashtagResponse = user.Do(() => user.Hashtag.GetPopularFeed(hashtag, rankToken));
                }
                else
                {
                    user.Log($"Load recent feed.");
                    isTop = 0;
                    hashtagResponse = user.Do(() => user.Hashtag.GetRecentFeed(hashtag, rankToken));
                }
            }
            else
            {
                if (Settings.Advanced.Comment.LoadPopularFeed)
                {
                    isTop = 1;
                    user.Log($"Load popular feed.");
                    hashtagResponse = user.Do(() => user.Hashtag.GetPopularFeed(hashtag, rankToken));
                }
                else if (Settings.Advanced.Comment.LoadRecentFeed)
                {
                    isTop = 0;
                    user.Log($"Load recent feed.");
                    hashtagResponse = user.Do(() => user.Hashtag.GetRecentFeed(hashtag, rankToken));
                }
                else
                {
                    isTop = 1;
                    user.Log($"Load popular feed.");
                    hashtagResponse = user.Do(() => user.Hashtag.GetPopularFeed(hashtag, rankToken));
                }
            }

            Utils.RandomSleep(3000, 5000);

            // Extract all media

            var medias =
                hashtagResponse.Sections.Where(s => s.LayoutType == "media_grid")
                    .Select(s => s.LayoutContent)
                    .Where(s => s.Medias.Any())
                    .Select(s => s.Medias);

            if (!medias.Any() && isTop == 0)
            {
                user.Log($"Hashtag #{hashtag} suspicious. Only popular medias are available.");
                user.Log($"Load popular feed.");
                isTop = 1;

                rankToken = Utils.GenerateUUID(true);
                hashtagResponse = user.Do(() => user.Hashtag.GetPopularFeed(hashtag, rankToken));
            }

            return medias;
        }

        private static void SaveLog(string data, string file = Variables.FileCommentsLog)
        {
            lock (DataLocker)
            {
                try
                {
                    using (TextWriter textWriter = new StreamWriter(file, true))
                    {
                        textWriter.WriteLine(data);
                    }
                }
                catch (Exception e)
                {
                    //throw new Exception();
                }
            }
        }

        private static string GetComment()
        {
            lock (DataLocker)
            {
                string comment;

                if (CommentsList.Count > _commentCounter)
                    comment = CommentsList[_commentCounter++];
                else
                {
                    _commentCounter = 0;
                    comment = CommentsList[_commentCounter++];
                }

                return Helpers.TextRandomizer.Randomize(comment);
            }
        }

        private static string GetTag()
        {
            lock (DataLocker)
            {
                if (TagList.Count > _tagCounter)
                    return TagList[_tagCounter++];

                _tagCounter = 0;
                return TagList[_tagCounter++];
            }
        }
    }
}
