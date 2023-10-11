using Microsoft.EntityFrameworkCore;

namespace Database.DirectSender
{
    public class UserDirectContext : DbContext
    {
        public DbSet<UserDirect> Users { get; set; }

        public UserDirectContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=bin/Database/usersForDirectSender.sqlite");
        }
    }
}
