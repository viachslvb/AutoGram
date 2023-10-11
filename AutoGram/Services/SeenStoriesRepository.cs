using System;
using System.Linq;
using Database;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public static class SeenStoriesRepository
    {
        private static readonly object Lock = new object();

        public static void Create(SeenStory story)
        {
            lock (Lock)
            {
                story.DateModified = DateTime.Now;

                using (var db = new SeenStoriesContext())
                {
                    db.SeenStories.Add(story);
                    db.SaveChanges();
                }
            }
        }

        public static bool AlreadyExists(string ownerPk)
        {
            lock (Lock)
            {
                using (var db = new SeenStoriesContext())
                {
                    return db.SeenStories
                        .AsNoTracking()
                        .Any(s => s.OwnerPk == ownerPk);
                }
            }
        }
    }
}
