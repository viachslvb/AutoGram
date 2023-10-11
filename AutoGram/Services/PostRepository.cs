using System;
using System.Linq;
using Database;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public static class PostRepository
    {
        private static readonly object Lock = new object();

        public static Database.Post GetPost()
        {
            lock (Lock)
                using (var db = new PostContext())
                {
                    return db.Posts
                        .AsNoTracking()
                        .Where(c => c.Used < 1)
                        .OrderBy(d => d.DateModified)
                        .FirstOrDefault();
                }
        }

        public static bool Any()
        {
            lock (Lock)
                using (var db = new PostContext())
                {
                    return db.Posts
                        .AsNoTracking()
                        .Any(c => c.Used < 1);
                }
        }

        public static Database.Post GetPostById(int id)
        {
            lock (Lock)
                using (var db = new PostContext())
                {
                    return db.Posts
                        .AsNoTracking()
                        .FirstOrDefault(x => x.Id == id);
                }
        }

        public static void Update(Database.Post post)
        {
            post.DateModified = DateTime.Now;

            lock (Lock)
                using (var db = new PostContext())
                {
                    db.Entry(post).State = EntityState.Modified;
                    db.SaveChanges();
                }
        }
    }
}
