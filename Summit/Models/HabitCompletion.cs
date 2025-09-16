using System.ComponentModel.DataAnnotations;

namespace SummitAPI.Models
{
    public class HabitCompletion
    {
        [Key] public Guid Id { get; set; }
        public Guid HabitId { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(10)] public string DayKey { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
