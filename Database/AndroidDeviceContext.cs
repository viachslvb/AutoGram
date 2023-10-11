using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AndroidDeviceContext : DbContext
    {
        public DbSet<AndroidDevice> AndroidDevices { get; set; }

        public AndroidDeviceContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AndroidDevice>()
                .HasOne(a => a.App)
                .WithOne(b => b.AndroidDevice)
                .HasForeignKey<InstagramApp>(a => a.Id);

            modelBuilder.Entity<AndroidDevice>()
                .HasOne(a => a.Status)
                .WithOne(b => b.AndroidDevice)
                .HasForeignKey<Status>(a => a.Id);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=bin/Database/androidDevices.sqlite");
        }
    }
}
