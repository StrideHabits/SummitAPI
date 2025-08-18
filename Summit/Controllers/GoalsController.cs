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
    public class GoalsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GoalsController(AppDbContext db) => _db = db;
        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("{habitId:int}")]
        public async Task<ActionResult<GoalResponseDto>> Get(int habitId)
        {
            var uid = CurrentUserId();
            var g = await _db.Goals.Include(g => g.Habit)
                .FirstOrDefaultAsync(g => g.HabitId == habitId && g.Habit.UserId == uid);
            if (g == null) return NotFound();
            return new GoalResponseDto(g.Id, g.HabitId, g.TargetPerPeriod, g.PeriodType);
        }

        [HttpPost("{habitId:int}")]
        public async Task<IActionResult> Upsert(int habitId, [FromBody] GoalUpsertDto dto)
        {
            var uid = CurrentUserId();
            var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == habitId && h.UserId == uid);
            if (habit == null) return BadRequest("Invalid habit.");

            var g = await _db.Goals.FirstOrDefaultAsync(x => x.HabitId == habitId);
            if (g == null)
            {
                g = new Goal { HabitId = habitId, TargetPerPeriod = dto.TargetPerPeriod, PeriodType = dto.PeriodType };
                _db.Goals.Add(g);
            }
            else
            {
                g.TargetPerPeriod = dto.TargetPerPeriod;
                g.PeriodType = dto.PeriodType;
            }

            await _db.SaveChangesAsync();
            return Ok(new GoalResponseDto(g.Id, g.HabitId, g.TargetPerPeriod, g.PeriodType));
        }

        [HttpDelete("{habitId:int}")]
        public async Task<IActionResult> Delete(int habitId)
        {
            var uid = CurrentUserId();
            var g = await _db.Goals.Include(x => x.Habit)
                .FirstOrDefaultAsync(x => x.HabitId == habitId && x.Habit.UserId == uid);
            if (g == null) return NotFound();
            _db.Goals.Remove(g);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
