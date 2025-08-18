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
    public class CompletionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CompletionsController(AppDbContext db) => _db = db;

        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompletionResponseDto>>> List(
            [FromQuery] int? habitId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var uid = CurrentUserId();

            var q = _db.Completions
                .Include(c => c.Habit)
                .Where(c => c.Habit.UserId == uid)
                .AsQueryable();

            if (habitId.HasValue)
                q = q.Where(c => c.HabitId == habitId.Value);
            if (start.HasValue)
                q = q.Where(c => c.CompletedOn >= start.Value.Date);
            if (end.HasValue)
                q = q.Where(c => c.CompletedOn <= end.Value.Date);

            var list = await q.OrderByDescending(c => c.CompletedOn)
                .Select(c => new CompletionResponseDto(c.Id, c.HabitId, c.CompletedOn.Date))
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompletionCreateDto dto)
        {
            var uid = CurrentUserId();
            var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == dto.HabitId && h.UserId == uid);
            if (habit == null) return BadRequest("Invalid habit.");

            var date = (dto.Date ?? DateTime.UtcNow).Date;
            var exists = await _db.Completions.AnyAsync(c => c.HabitId == habit.Id && c.CompletedOn == date);
            if (exists) return Conflict("Already completed for this date.");

            var comp = new Completion { HabitId = habit.Id, CompletedOn = date };
            _db.Completions.Add(comp);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(List), new { habitId = habit.Id, start = date, end = date },
                new CompletionResponseDto(comp.Id, comp.HabitId, comp.CompletedOn));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = CurrentUserId();
            var comp = await _db.Completions.Include(c => c.Habit)
                .FirstOrDefaultAsync(c => c.Id == id && c.Habit.UserId == uid);
            if (comp == null) return NotFound();

            _db.Completions.Remove(comp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
