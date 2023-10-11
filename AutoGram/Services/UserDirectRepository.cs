using System.Linq;
using Database.DirectSender;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class UserDirectRepository
    {
        private static readonly object Lock = new object();

        public static UserDirect GetUser()
        {
            lock (Lock)
                using (var db = new UserDirectContext())
                {
                    var user = db.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => !u.IsProcessed);

                    MarkAsProcessed(user);
                    return user;
                }
        }

        public static bool Any()
        {
            lock (Lock)
                using (var db = new UserDirectContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .Any(u => !u.IsProcessed);
                }
        }

        public static bool AlreadyExists(UserDirect user)
        {
            lock (Lock)
                using (var db = new UserDirectContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .Any(u => u.Pk == user.Pk);
                }
        }

        public static void MarkAsProcessed(UserDirect user)
        {
            user.IsProcessed = true;

            lock (Lock)
                using (var db = new UserDirectContext())
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
        }
    }
}
