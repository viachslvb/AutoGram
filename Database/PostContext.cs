using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class PostContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=Posts/database.sqlite");
        }
    }
}
