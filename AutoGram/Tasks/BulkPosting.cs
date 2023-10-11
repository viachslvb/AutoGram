using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Services;
using AutoGram.Task.SubTask;

namespace AutoGram.Task
{
    class BulkPosting
    {
        public static void Do(Worker worker, Instagram.Instagram user)
        {
            #region Settings
            int randomLimit = Settings.Basic.Post.SendFromEach;
            if (Settings.IsAdvanced)
            {
                if (Settings.Advanced.Post.RandomLimit.Use)
                {
                    randomLimit = Utils.Random.Next(
                        Settings.Advanced.Post.RandomLimit.From,
                        Settings.Advanced.Post.RandomLimit.To);
                }

                if (Settings.Advanced.PostAfterRegistration.Use)
                {
                    randomLimit = Utils.Random.Next(
                        Settings.Advanced.PostAfterRegistration.From,
                        Settings.Advanced.PostAfterRegistration.To);
                }
            }

            int delayFrom = Settings.Basic.General.PauseFrom;
            int delayTo = Settings.Basic.General.PauseTo;

            if (Settings.IsAdvanced && Settings.Advanced.PostAfterRegistration.Use)
            {
                delayFrom = Settings.Advanced.PostAfterRegistration.Delay.From;
                delayTo = Settings.Advanced.PostAfterRegistration.Delay.To;
            }

            if (randomLimit < 1) return;

            if(Settings.Advanced.Post.RequireOnce &&
                user.Storage.IsPostedMedia) return;
            #endregion

            // Upload Photos
            var photosList = Settings.Advanced.Post.Type.UseVideoFromFolder
                ? Photos.GetVideoList(worker.Folder)
                : Photos.GetPhotosList(worker.Folder);
            var photos = new Photos(photosList);
            var num = 0;
            int errorsCount = 0;

            while (true)
            {
                // Skip the account if bad
                if (worker.Account.Skip)
                {
                    worker.SkipActivate();
                    throw new SuspendExecutionException();
                }

                try
                {
                    var media = new List<MediaObject>();
                    MediaType mediaType;
                    Database.Post databasePost = null;

                    #region Post settings

                    bool fromDatabase = false;
                    string captionFromDatabase = string.Empty;

                    if (Settings.Advanced.Post.UsePostsDatabase)
                    {
                        if (PostRepository.Any())
                        {
                            databasePost = PostRepository.GetPost();
                            databasePost.Used++;

                            PostRepository.Update(databasePost);

                            if (!Settings.Advanced.Post.UsePostsDatabaseCaptionsOnly)
                            {
                                fromDatabase = true;
                            }
                            else
                            {
                                captionFromDatabase = databasePost.Caption;
                            }
                        }
                        else
                        {
                            user.Log("Post repository are used.");
                            throw new SuspendThreadWorkException();
                        }
                    }

                    if (fromDatabase)
                    {
                        var post = new PostData(databasePost.PictureLocalPath, databasePost.Caption);

                        if (Settings.Advanced.Post.Type.SlidePosting)
                        {
                            media.Add(post);
                            media.Add(post);
                            mediaType = MediaType.Album;
                        }
                        else
                        {
                            media.Add(post);
                            mediaType = MediaType.Photo;
                        }
                    }
                    else if (Settings.Advanced.Post.Type.RandomizePostingType)
                    {
                        var mediaTypesList = new List<MediaType>
                                    {
                                        MediaType.Photo,
                                        MediaType.Album,
                                        MediaType.Video
                                    };

                        mediaType = Settings.Advanced.Post.Type.EnableRandomizingVideoType
                            ? mediaTypesList[Utils.Random.Next(3)]
                            : mediaTypesList[Utils.Random.Next(2)];

                        switch (mediaType)
                        {
                            case MediaType.Photo:
                                media.Add(new Photo(photos.Get(), isProfileUrl: user.IsProfileUrl));
                                break;
                            case MediaType.Album:
                                var currentPhoto = photos.Get();
                                media.Add(new Photo(currentPhoto, isProfileUrl: user.IsProfileUrl));
                                media.Add(Settings.Advanced.Post.Type.DifferentSlidePhotos
                                    ? new Photo(photos.Get(), isProfileUrl: user.IsProfileUrl)
                                    : new Photo(currentPhoto, isProfileUrl: user.IsProfileUrl));
                                break;
                            case MediaType.Video:
                                media.Add(new Photo(photos.Get(), true, isProfileUrl: user.IsProfileUrl));
                                break;
                        }
                    }
                    else
                    {
                        if (Settings.Advanced.Post.Type.SlidePosting)
                        {
                            var currentPhoto = photos.Get();
                            media.Add(new Photo(currentPhoto, isProfileUrl: user.IsProfileUrl));
                            media.Add(Settings.Advanced.Post.Type.DifferentSlidePhotos
                                ? new Photo(photos.Get(), isProfileUrl: user.IsProfileUrl)
                                : new Photo(currentPhoto, isProfileUrl: user.IsProfileUrl));
                            mediaType = MediaType.Album;
                        }
                        else
                        {
                            if (Settings.Basic.Image.MakeVideo)
                            {
                                if (Settings.Advanced.Post.Type.UseVideoFromFolder)
                                {
                                    media.Add(new Video(photos.Get()));
                                }
                                else
                                {
                                    media.Add(new Photo(photos.Get(), true, isProfileUrl: user.IsProfileUrl));

                                }
                                mediaType = MediaType.Video;
                            }
                            else
                            {
                                media.Add(new Photo(photos.Get(), isProfileUrl: user.IsProfileUrl));
                                mediaType = MediaType.Photo;
                            }
                        }
                    }

                    #endregion

                    if (Settings.Advanced.Post.UsePostsDatabaseCaptionsOnly &&
                        !string.IsNullOrEmpty(captionFromDatabase))
                    {
                        media[0].Caption = captionFromDatabase;
                    }

                    var uploadResponse = Post.Do(user, media, mediaType, databasePost);
                    //worker.Account.UpdateSentCount(++num);
                    errorsCount = 0;

                    // Check availability post
                    if (Settings.Advanced.Post.SuspentIfPostAutoDeleted)
                    {
                        if (!MediaTools.MediaIsPublished(uploadResponse.Media.Code))
                        {
                            user.Log("This post has been automatically deleted by Instagram.");
                            user.Storage.IsPostsDeletedAutomatically = true;
                            break;
                        }
                    }

                    // Add comment
                    if (Settings.Advanced.Post.Content.AddComment)
                    {
                        string comment = media.FirstOrDefault().Comment;
                        if (!string.IsNullOrEmpty(comment))
                        {
                            Thread.Sleep(5000);

                            string mediaId = uploadResponse.Media.GetId();
                            AddComment.Do(user, mediaId, comment);
                        }
                    }

                    user.Storage.IsPostedMedia = true;
                }
                catch (UploadPostFailedException exception)
                {
                    if (errorsCount >= Variables.ErrorLimitPosting)
                    {
                        user.Log(exception.Message);
                        throw new SuspendExecutionException();
                    }

                    Utils.RandomSleep(4000, 7000);
                    errorsCount++;
                    continue;
                }

                if (user.Storage.EmptyCounter >= Settings.Basic.Limit.EmptyCaption)
                {
                    user.Activity.Status = "Apparently the account shadow banned.";
                    worker.Account.WriteLog(user.Activity.Status);

                    break;
                }

                if (num >= randomLimit)
                {
                    break;
                }

                var sleepTime = Utils.Random.Next(delayFrom,
                    delayTo);
                worker.Account.WriteLog($"Sleep {sleepTime}s.");
                Thread.Sleep(sleepTime * 1000);
            }
        }
    }
}
