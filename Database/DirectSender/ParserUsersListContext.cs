using Microsoft.EntityFrameworkCore;

namespace Database.DirectSender
{
    public class ParserUsersListContext : DbContext
    {
        public DbSet<UserDirect> Users { get; set; }

        public ParserUsersListContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=bin/Database/parserUsersList.sqlite");
        }
    }
}
