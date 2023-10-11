using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using AutoGram.Instagram.Exception;

namespace AutoGram.Task.SubTask
{
    static class Stories
    {
        private static readonly List<StoryMedia> StoriesMedia;
        private static readonly object UploadingLock = new object();

        static Stories()
        {
            StoriesMedia = new List<StoryMedia>();

            foreach (var story in Directory.GetFiles(Variables.FolderStoriesMedia)
                .Where(
                        file =>
                            Path.GetExtension(file) == ".jpg" || Path.GetExtension(file) == ".png" ||
                            Path.GetExtension(file) == ".jpeg" || Path.GetExtension(file) == ".mp4"))
            {
                bool isVideo = Path.GetExtension(story) == ".mp4";

                StoriesMedia.Add(new StoryMedia { Path = story, IsVideo = isVideo });
            }
        }

        public static void UploadStories(Instagram.Instagram user)
        {
            if (user.Storage.IsUploadedStories) return;
            user.Log("Uploading stories.");

            user.Do(() => user.Account.SetReelSettings());

            lock (UploadingLock)
            {
                foreach (var story in StoriesMedia)
                {
                    var media = story.IsVideo
                        ? (MediaObject)new Video(story.Path)
                        : new Photo(story.Path, isDirectPhoto: true);

                    var response = story.IsVideo
                        ? user.Do(() => user.Internal.UploadVideo(media, isStory: true))
                        : user.Do(() => user.Internal.UploadPhoto(media, isStory: true));

                    if (response.IsOk())
                    {
                        user.Storage.IsUploadedStories = true;
                        user.Log($"Uploaded story {response.Media.Id}");

                        if (Settings.Advanced.Profile.Stories.AddToHighlights)
                        {
                            Thread.Sleep(5000);

                            user.Do(() => user.Internal.HighLightsTray());

                            user.Log("Sleep 5 s.");
                            Thread.Sleep(5000);

                            try
                            {
                                var highlightResponse = user.Do(() => user.Highlights.CreateHighlight(response.Media.Id,
                                    Settings.Advanced.Profile.Stories.HighlightTitle), true);

                                if (highlightResponse.IsOk())
                                    user.Log($"Added story to highlight {response.Media.Id}");
                            }
                            catch (IgnoreJsonErrorsException)
                            {
                            }
                        }

                        user.Log("Sleep 5 s.");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        user.Log(response.IsMessage()
                        ? response.GetMessage()
                        : "Undefined error story uploading.");
                    }
                }
            }
        }

        public static void SeeFeedStories()
        {

        }
    }

    class StoryMedia
    {
        public string Path { get; set; }
        public bool IsVideo { get; set; }
    }
}
