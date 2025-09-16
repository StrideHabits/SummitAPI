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
    public class HabitsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HabitsController(AppDbContext db) { _db = db; }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/habits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetHabits()
        {
            var items = await _db.Habits
                .Where(h => h.UserId == UserId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HabitDto(h.Id, h.Name, h.Frequency, h.Tag, h.ImageUrl, h.CreatedAt, h.UpdatedAt))
                .ToListAsync();

            return items;
        }

        // POST /api/habits
        [HttpPost]
        public async Task<ActionResult<HabitDto>> Create([FromBody] HabitCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                Name = dto.Name.Trim(),
                Frequency = Math.Max(0, dto.Frequency),
                Tag = string.IsNullOrWhiteSpace(dto.Tag) ? null : dto.Tag.Trim(),
                ImageUrl = dto.ImageUrl
            };

            _db.Habits.Add(habit);
            await _db.SaveChangesAsync();

            var result = new HabitDto(habit.Id, habit.Name, habit.Frequency, habit.Tag, habit.ImageUrl, habit.CreatedAt, habit.UpdatedAt);
            return CreatedAtAction(nameof(GetHabits), new { id = habit.Id }, result);
        }
    }
}
