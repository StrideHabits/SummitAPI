using System.ComponentModel.DataAnnotations;

namespace SummitAPI.Models
{
    public class User
    {
        [Key] public Guid Id { get; set; }
        [MaxLength(128)] public string Name { get; set; } = "";
        [MaxLength(256)] public string Email { get; set; } = "";
        [MaxLength(512)] public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
