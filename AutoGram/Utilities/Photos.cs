using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoGram
{
    class Photos
    {
        private static readonly Queue<PhotoFolder> FoldersLite = new Queue<PhotoFolder>();
        private static readonly Queue<PhotoFolder> FoldersHard = new Queue<PhotoFolder>();
        private static readonly Random Random = new Random();

        static Photos()
        {
            ImportPhotos();
        }

        private List<string> _photosList;
        private int _counter;
        
        public Photos(List<string> photosList)
        {
            _photosList = photosList;
        }

        public string Get()
        {
            if (_photosList.Count > _counter)
                return _photosList[_counter++];

            _counter = 0;
            return _photosList[_counter++];
        }

        public static void ImportPhotos()
        {
            void FillQueue(Queue<PhotoFolder> queue, IEnumerable<string> list, bool isLite)
            {
                if (queue.Any()) queue.Clear();

                foreach (var e in list)
                {
                    queue.Enqueue(new PhotoFolder(e, isLite));
                }
            }

            // Lite photos
            var directoriesLite = Directory.GetDirectories(Variables.FolderPhotos + "/" + Variables.FolderPhotosLite);
            FillQueue(FoldersLite, directoriesLite, true);

            // Hard photos
            var directoriesHard = Directory.GetDirectories(Variables.FolderPhotos + "/" + Variables.FolderPhotosHard);
            FillQueue(FoldersHard, directoriesHard, false);
        }

        public static bool Any(bool isLite)
        {
            return isLite ? FoldersLite.Any() : FoldersHard.Any();
        }

        public static PhotoFolder ReserveFolder(bool isLite)
        {
            if (isLite && !FoldersLite.Any() || !isLite && !FoldersHard.Any()) return null;
            return isLite ? FoldersLite.Dequeue() : FoldersHard.Dequeue();
        }

        public static void UnreserveFolder(PhotoFolder folder)
        {
            if (folder.IsLite) FoldersLite.Enqueue(folder);
            else FoldersHard.Enqueue(folder);
        }

        public static List<string> GetPhotosList(PhotoFolder folder)
        {
            var photos = Directory.GetFiles(folder.Path)
                            .Where(
                                fileImage =>
                                    Path.GetExtension(fileImage) == ".jpg" ||
                                    Path.GetExtension(fileImage) == ".png" ||
                                    Path.GetExtension(fileImage) == ".jpeg")
                            .ToList();
            photos.Shuffle();

            return photos;
        }

        public static List<string> GetVideoList(PhotoFolder folder)
        {
            var videos = Directory.GetFiles(folder.Path)
                            .Where(
                                fileImage =>
                                    Path.GetExtension(fileImage) == ".mp4")
                            .ToList();
            videos.Shuffle();

            return videos;
        }

        public static string GetProfilePhoto(PhotoFolder folder)
        {
            var photos =
                Directory.GetFiles(folder.Path + "/" + Variables.FolderProfilePhoto + "/")
                    .Where(
                        fileImage =>
                            Path.GetExtension(fileImage) == ".jpg" || Path.GetExtension(fileImage) == ".png" ||
                            Path.GetExtension(fileImage) == ".jpeg")
                    .ToList();

            return photos[Random.Next(0, photos.Count - 1)];
        }
    }

    class PhotoFolder
    {
        public string Path;
        public bool IsLite;

        public PhotoFolder(string path, bool isLite)
        {
            Path = path;
            IsLite = isLite;
        }
    }
}
