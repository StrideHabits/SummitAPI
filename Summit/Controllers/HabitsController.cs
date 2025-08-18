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
    public class HabitsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HabitsController(AppDbContext db) => _db = db;

        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitResponseDto>>> List([FromQuery] bool includeStats = true)
        {
            var uid = CurrentUserId();
            var habits = await _db.Habits
                .Where(h => h.UserId == uid)
                .Include(h => h.Completions)
                .ToListAsync();

            var result = habits.Select(h =>
            {
                int currentStreak = CalcCurrentStreak(h.Completions);
                int longestStreak = CalcLongestStreak(h.Completions);
                double completionRate = CalcCompletionRate(h.Completions, h.CreatedAt);

                return new HabitResponseDto(h.Id, h.Title, h.Frequency, h.CreatedAt, h.TagId,
                    includeStats ? currentStreak : 0,
                    includeStats ? longestStreak : 0,
                    includeStats ? completionRate : 0);
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HabitResponseDto>> Get(int id)
        {
            var uid = CurrentUserId();
            var h = await _db.Habits.Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == uid);
            if (h == null) return NotFound();

            var dto = new HabitResponseDto(
                h.Id, h.Title, h.Frequency, h.CreatedAt, h.TagId,
                CalcCurrentStreak(h.Completions),
                CalcLongestStreak(h.Completions),
                CalcCompletionRate(h.Completions, h.CreatedAt)
            );
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HabitCreateDto dto)
        {
            var uid = CurrentUserId();
            if (dto.TagId.HasValue &&
                !await _db.Tags.AnyAsync(t => t.Id == dto.TagId.Value && t.UserId == uid))
                return BadRequest("Invalid tag.");

            var h = new Habit
            {
                UserId = uid,
                Title = dto.Title.Trim(),
                Frequency = string.IsNullOrWhiteSpace(dto.Frequency) ? "daily" : dto.Frequency.Trim(),
                TagId = dto.TagId
            };
            _db.Habits.Add(h);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = h.Id }, new { h.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] HabitUpdateDto dto)
        {
            var uid = CurrentUserId();
            var h = await _db.Habits.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (h == null) return NotFound();

            if (dto.TagId.HasValue &&
                !await _db.Tags.AnyAsync(t => t.Id == dto.TagId.Value && t.UserId == uid))
                return BadRequest("Invalid tag.");

            h.Title = dto.Title.Trim();
            h.Frequency = dto.Frequency.Trim();
            h.TagId = dto.TagId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = CurrentUserId();
            var h = await _db.Habits.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (h == null) return NotFound();
            _db.Habits.Remove(h);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id:int}/stats")]
        public async Task<IActionResult> Stats(int id)
        {
            var uid = CurrentUserId();
            var h = await _db.Habits.Include(x => x.Completions)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (h == null) return NotFound();

            var stats = new
            {
                currentStreak = CalcCurrentStreak(h.Completions),
                longestStreak = CalcLongestStreak(h.Completions),
                completionRate = CalcCompletionRate(h.Completions, h.CreatedAt)
            };
            return Ok(stats);
        }

        // --- simple streak helpers
        private static int CalcCurrentStreak(IEnumerable<Completion> comps)
        {
            var days = comps.Select(c => c.CompletedOn.Date).Distinct().ToHashSet();
            int streak = 0;
            for (var d = DateTime.UtcNow.Date; days.Contains(d); d = d.AddDays(-1)) streak++;
            return streak;
        }

        private static int CalcLongestStreak(IEnumerable<Completion> comps)
        {
            var days = comps.Select(c => c.CompletedOn.Date).Distinct().OrderBy(d => d).ToList();
            int best = 0, cur = 0;
            DateTime? prev = null;
            foreach (var d in days)
            {
                if (prev.HasValue && d == prev.Value.AddDays(1)) cur++;
                else cur = 1;
                best = Math.Max(best, cur);
                prev = d;
            }
            return best;
        }

        private static double CalcCompletionRate(IEnumerable<Completion> comps, DateTime createdAt)
        {
            var totalDays = Math.Max(1, (DateTime.UtcNow.Date - createdAt.Date).Days + 1);
            var completedDays = comps.Select(c => c.CompletedOn.Date).Distinct().Count();
            return Math.Round((double)completedDays / totalDays, 3);
        }
    }
}
