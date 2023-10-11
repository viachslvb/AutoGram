using System.Collections.Generic;
using System.Linq;
using Database.DirectSender;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class UsersDirectRejectedRepository
    {
        private static readonly object Lock = new object();

        public static bool AlreadyExists(UserDirect user)
        {
            lock (Lock)
                using (var db = new DirectBlacklistContext("usersDirectRejected.sqlite"))
                {
                    return db.Users
                        .AsNoTracking()
                        .Any(u => u.Pk == user.Pk);
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
                using (var db = new DirectBlacklistContext("usersDirectRejected.sqlite"))
                {
                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
            }
        }

        public static void AddRange(List<UserDirect> users)
        {
            lock (Lock)
            {
                using (var db = new DirectBlacklistContext("usersDirectRejected.sqlite"))
                {
                    var dbUsers = db.Users.AsNoTracking();

                    var newUsers = users.Where(x => !dbUsers.Any(y => y.Pk == x.Pk)).ToList();

                    db.Users.AddRange(newUsers);
                    db.SaveChanges();
                }
            }
        }

        public static UserDirect GetByPk(string pk)
        {
            lock (Lock)
            {
                using (var db = new DirectBlacklistContext("usersDirectRejected.sqlite"))
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
                using (var db = new DirectBlacklistContext("usersDirectRejected.sqlite"))
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
        }
    }
}
