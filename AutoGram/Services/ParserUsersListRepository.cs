using System.Linq;
using Database.DirectSender;
using Microsoft.EntityFrameworkCore;

namespace AutoGram.Services
{
    public class ParserUsersListRepository
    {
        private static readonly object Lock = new object();

        public static bool AlreadyExists(UserDirect user)
        {
            lock (Lock)
                using (var db = new ParserUsersListContext())
                {
                    return db.Users
                        .AsNoTracking()
                        .Any(u => u.Pk == user.Pk);
                }
        }

        public static void Create(UserDirect user)
        {
            lock (Lock)
            {
                using (var db = new ParserUsersListContext())
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
            }
        }
    }
}
