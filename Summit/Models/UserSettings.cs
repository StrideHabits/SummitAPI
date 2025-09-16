using System.ComponentModel.DataAnnotations;

namespace SummitAPI.Models
{
    public class UserSettings
    {
        [Key] public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [MaxLength(16)] public string Theme { get; set; } = "light";
        public bool Notifications { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
