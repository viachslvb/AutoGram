using Microsoft.EntityFrameworkCore;

namespace Database.DirectSender
{
    public class DirectBlacklistContext : DbContext
    {
        public DbSet<UserDirect> Users { get; set; }

        private string _database;

        public DirectBlacklistContext(string database = "usersDirectBlacklist.sqlite")
        {
            _database = database;

            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=bin/Database/{_database}");
        }
    }
}
