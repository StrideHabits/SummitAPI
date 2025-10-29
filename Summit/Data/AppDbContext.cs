using Microsoft.EntityFrameworkCore;
using SummitAPI.Models;
using System.Linq;

namespace SummitAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();
        public DbSet<UserSettings> Settings => Set<UserSettings>();
        public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Users
            b.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Habits
            b.Entity<Habit>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Concurrency tokens
            b.Entity<Habit>()
                .Property(x => x.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

            b.Entity<HabitCompletion>()
                .Property(x => x.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

            // HabitCompletions
            b.Entity<HabitCompletion>()
                .HasIndex(c => new { c.HabitId, c.DayKey })
                .IsUnique();

            b.Entity<HabitCompletion>()
                .HasOne<Habit>()
                .WithMany()
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            // For change feed scans
            b.Entity<HabitCompletion>()
                .HasIndex(c => c.UpdatedAt);

            // Settings
            b.Entity<UserSettings>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Idempotency cache
            b.Entity<RequestLog>()
                .HasIndex(x => new { x.UserId, x.RequestId })
                .IsUnique();
        }

        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var e in ChangeTracker.Entries())
            {
                if (e.State == EntityState.Added)
                {
                    if (e.Metadata.FindProperty("CreatedAt") != null)
                        e.CurrentValues["CreatedAt"] = now;
                }

                if (e.State == EntityState.Added || e.State == EntityState.Modified)
                {
                    if (e.Metadata.FindProperty("UpdatedAt") != null)
                        e.CurrentValues["UpdatedAt"] = now;
                }
            }
        }
    }
}
