using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoGram.Instagram.Exception;
using AutoGram.Instagram.Response;
using AutoGram.Services;

namespace AutoGram.Task.SubTask
{
    public enum MediaType
    {
        Photo,
        Album,
        Video,
    }

    static class Post
    {
        public static MediaConfigureResponse Do(Instagram.Instagram user, List<MediaObject> media, MediaType mediaType, Database.Post post = null)
        {
            if(!media.Any())
                throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null);

            MediaConfigureResponse response;
            switch (mediaType)
            {
                case MediaType.Photo:
                    response = user.Timeline.UploadPhoto(media.FirstOrDefault());
                    break;
                case MediaType.Album:
                    response = user.Timeline.UploadAlbum(media);
                    break;
                case MediaType.Video:
                    response = user.Timeline.UploadVideo(media.FirstOrDefault());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null);
            }

            if (response.IsOk())
            {
                user.Log($"Media was successfully uploaded.");
                user.Activity.Sent.Total++;

                if (!string.IsNullOrEmpty(media.FirstOrDefault()?.Caption) && !response.Media.IsCaption())
                {
                    user.Log("But a caption is empty.");
                    user.Storage.EmptyCounter++;
                    user.Activity.Sent.Error++;

                    if (post != null)
                    {
                        post.Consumed = true;
                        PostRepository.Update(post);
                    }
                }
                else
                {
                    user.Activity.Sent.Success++;
                    user.Storage.EmptyCounter = 0;
                }

                return response;
            }

            string errorMessage = response.IsMessage()
                    ? response.GetMessage()
                    : "Upload post failed. Undefined error.";

            Log.Write(errorMessage, LogResource.Post);
            throw new UploadPostFailedException(errorMessage);
        }
    }
}
