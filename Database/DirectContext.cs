using System;
using System.Collections.Generic;
using System.Diagnostics;
using Database.Direct;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DirectContext : DbContext
    {
        public DbSet<DirectThread> DirectThreads { get; set; }

        private readonly string _databaseFile;

        public DirectContext(string databaseFile)
        {
            _databaseFile = databaseFile;

            Database.EnsureCreated();

            DateTime defaultDateTime = new DateTime();

            Dictionary<string, string> columnNameToAddColumnSql = new Dictionary<string, string>
            {
                {
                    "Column1",
                    $"ALTER TABLE DirectThreads ADD COLUMN DateCreated TEXT NOT NULL DEFAULT '{defaultDateTime}'"
                },
                {
                    "Column2",
                    $"ALTER TABLE DirectThreads ADD COLUMN DateModified TEXT NOT NULL DEFAULT '{defaultDateTime}'"
                }
            };

            foreach (var pair in columnNameToAddColumnSql)
            {
                string sql = pair.Value;

                try
                {
                    this.Database.ExecuteSqlCommand(sql);
                }
                catch (Exception)
                {
                    //Debug.WriteLine(e.Message);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DirectThread>()
                .HasMany(a => a.Messages)
                .WithOne(b => b.Thread)
                .HasForeignKey(a => a.ThreadForeignKey);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databaseFile}");

            
        }
    }
}
