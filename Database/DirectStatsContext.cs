using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DirectStatsContext : DbContext
    {
        public DbSet<DirectStatsModel> DirectStats { get; set; }

        public DirectStatsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=bin/Database/directStats.sqlite");
        }
    }
}