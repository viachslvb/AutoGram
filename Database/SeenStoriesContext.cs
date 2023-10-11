using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class SeenStoriesContext : DbContext
    {
        public DbSet<SeenStory> SeenStories { get; set; }

        //public SeenStoriesContext()
        //{
        //    Database.EnsureCreated();
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=bin/Database/seenStories.sqlite");
        }
    }
}
