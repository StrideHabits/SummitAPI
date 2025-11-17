using System.ComponentModel.DataAnnotations;

namespace SummitAPI.Models
{
    public class RequestLog
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        [MaxLength(64)] public string RequestId { get; set; } = ""; // from client
        public string ResultJson { get; set; } = "";                 // cached response
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
