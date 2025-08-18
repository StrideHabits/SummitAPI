using System.Security.Claims;
using Summit.Data;
using Summit.DTOs;
using Summit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Summit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotesController(AppDbContext db) => _db = db;
        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("{completionId:int}")]
        public async Task<IEnumerable<NoteResponseDto>> List(int completionId)
        {
            var uid = CurrentUserId();
            return await _db.Notes
                .Include(n => n.Completion).ThenInclude(c => c.Habit)
                .Where(n => n.CompletionId == completionId && n.Completion.Habit.UserId == uid)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteResponseDto(n.Id, n.CompletionId, n.Content, n.CreatedAt))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteCreateDto dto)
        {
            var uid = CurrentUserId();
            var comp = await _db.Completions.Include(c => c.Habit)
                .FirstOrDefaultAsync(c => c.Id == dto.CompletionId && c.Habit.UserId == uid);
            if (comp == null) return BadRequest("Invalid completion.");

            var note = new Note { CompletionId = comp.Id, Content = dto.Content.Trim() };
            _db.Notes.Add(note);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(List), new { completionId = comp.Id },
                new NoteResponseDto(note.Id, note.CompletionId, note.Content, note.CreatedAt));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = CurrentUserId();
            var note = await _db.Notes.Include(n => n.Completion).ThenInclude(c => c.Habit)
                .FirstOrDefaultAsync(n => n.Id == id && n.Completion.Habit.UserId == uid);
            if (note == null) return NotFound();
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
