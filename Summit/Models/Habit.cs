using System.ComponentModel.DataAnnotations;

namespace SummitAPI.Models
{
    public class Habit
    {
        [Key] public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [MaxLength(128)] public string Name { get; set; } = "";
        public int Frequency { get; set; } = 0;         // completions per week
        [MaxLength(64)] public string? Tag { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
