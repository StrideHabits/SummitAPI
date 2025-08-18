using Summit.Models;
using Microsoft.EntityFrameworkCore;

namespace Summit.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<Completion> Completions => Set<Completion>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Goal> Goals => Set<Goal>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Attachment> Attachments => Set<Attachment>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // User.Username unique
            b.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Habit relations
            b.Entity<Habit>()
                .HasOne(h => h.User)
                .WithMany(u => u.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Habit>()
                .HasOne(h => h.Tag)
                .WithMany(t => t.Habits)
                .HasForeignKey(h => h.TagId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique: one goal per habit
            b.Entity<Goal>()
                .HasIndex(g => g.HabitId)
                .IsUnique();

            // Completion unique per (Habit, Date)
            b.Entity<Completion>()
                .HasIndex(c => new { c.HabitId, c.CompletedOn })
                .IsUnique();

            // Attachments/Notes
            b.Entity<Attachment>()
                .HasOne(a => a.Completion)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CompletionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Note>()
                .HasOne(n => n.Completion)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.CompletionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tag owner
            b.Entity<Tag>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tags)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
