using System.Collections.Generic;
using System.Linq;
using Database.DirectSender;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class UsersDirectBlacklistRepository
    {
        private static readonly object Lock = new object();

        public static bool AlreadyExists(UserDirect user)
        {
            lock (Lock)
                using (var db = new DirectBlacklistContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .Any(u => u.Pk == user.Pk);
                }
        }

        public static List<UserDirect> GetAll()
        {
            lock (Lock)
            {
                using (var db = new DirectBlacklistContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .ToList();
                }
            }
        }

        public static void AddRange(List<UserDirect> users)
        {
            lock (Lock)
            {
                using (var db = new DirectBlacklistContext())
                {
                    db.Users
                        .AddRange(users);
                    db.SaveChanges();
                }
            }
        }

        public static void Create(UserDirect user)
        {
            var newUser = new UserDirect
            {
                Pk = user.Pk, FullName = user.FullName, IsPrivate = user.IsPrivate, Username = user.Username
            };

            lock (Lock)
            {
                using (var db = new DirectBlacklistContext())
                {
                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
            }
        }

        public static UserDirect GetByPk(string pk)
        {
            lock (Lock)
            {
                using (var db = new DirectBlacklistContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Pk == pk);
                }
            }
        }

        public static void MarkAsProcessed(UserDirect user)
        {
            if (user.Id == 0)
            {
                user = GetByPk(user.Pk);
            }

            user.IsProcessed = true;

            lock (Lock)
                using (var db = new DirectBlacklistContext())
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
        }
    }
}
