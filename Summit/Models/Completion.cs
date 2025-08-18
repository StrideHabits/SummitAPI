using System.Net.Mail;

namespace Summit.Models
{
    public class Completion
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime CompletedOn { get; set; } = DateTime.UtcNow.Date; // store date part

        public Habit Habit { get; set; } = default!;
        public List<Note> Notes { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();
    }
}
