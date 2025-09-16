using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummitAPI.Data;
using SummitAPI.Dtos;
using SummitAPI.Models;
using System.Security.Claims;

namespace SummitAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CheckInsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CheckInsController(AppDbContext db) { _db = db; }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/checkins?sinceUtc=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CheckInDto>>> GetAll([FromQuery] DateTime? sinceUtc = null)
        {
            var userHabitIds = await _db.Habits.Where(h => h.UserId == UserId)
                                               .Select(h => h.Id)
                                               .ToListAsync();

            var q = _db.HabitCompletions.Where(c => userHabitIds.Contains(c.HabitId));
            if (sinceUtc.HasValue) q = q.Where(c => c.UpdatedAt >= sinceUtc.Value);

            var items = await q.OrderByDescending(c => c.CompletedAt)
                               .Select(c => new CheckInDto(c.Id, c.HabitId, c.CompletedAt, c.DayKey))
                               .ToListAsync();

            return items;
        }

        // POST /api/checkins  (idempotent per (habitId, dayKey))
        [HttpPost]
        public async Task<ActionResult<CheckInDto>> Create([FromBody] CheckInCreateDto dto)
        {
            var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == dto.HabitId && h.UserId == UserId);
            if (habit is null) return NotFound("Habit not found.");

            var completedAt = dto.CompletedAt?.ToUniversalTime() ?? DateTime.UtcNow;
            var dayKey = dto.DayKey ?? completedAt.ToString("yyyy-MM-dd");

            var existing = await _db.HabitCompletions
                .FirstOrDefaultAsync(c => c.HabitId == habit.Id && c.DayKey == dayKey);

            if (existing is null)
            {
                var c = new HabitCompletion
                {
                    Id = Guid.NewGuid(),
                    HabitId = habit.Id,
                    CompletedAt = completedAt,
                    DayKey = dayKey
                };
                _db.HabitCompletions.Add(c);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAll), new { id = c.Id }, new CheckInDto(c.Id, c.HabitId, c.CompletedAt, c.DayKey));
            }
            else
            {
                existing.CompletedAt = completedAt;
                existing.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(new CheckInDto(existing.Id, existing.HabitId, existing.CompletedAt, existing.DayKey));
            }
        }
    }
}
