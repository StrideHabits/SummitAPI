using Microsoft.EntityFrameworkCore;
using SummitAPI.Models;

namespace SummitAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();
        public DbSet<UserSettings> Settings => Set<UserSettings>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>().HasIndex(u => u.Email).IsUnique();

            b.Entity<Habit>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Idempotency per (HabitId, DayKey)
            b.Entity<HabitCompletion>()
                .HasIndex(c => new { c.HabitId, c.DayKey })
                .IsUnique();

            b.Entity<HabitCompletion>()
                .HasOne<Habit>()
                .WithMany()
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<UserSettings>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
