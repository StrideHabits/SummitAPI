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
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SettingsController(AppDbContext db) { _db = db; }
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<SettingsDto>> Get()
        {
            var cfg = await _db.Configurations.FirstOrDefaultAsync(c => c.UserId == UserId);
            if (cfg is null)
            {
                cfg = new Configuration { Id = Guid.NewGuid(), UserId = UserId };
                _db.Configurations.Add(cfg);
                await _db.SaveChangesAsync();
            }
            return new SettingsDto(cfg.Theme, cfg.Notifications);
        }

        [HttpPut]
        public async Task<ActionResult<SettingsDto>> Update([FromBody] SettingsDto dto)
        {
            var cfg = await _db.Configurations.FirstOrDefaultAsync(c => c.UserId == UserId)
                      ?? new Configuration { Id = Guid.NewGuid(), UserId = UserId };
            cfg.Theme = string.IsNullOrWhiteSpace(dto.Theme) ? "light" : dto.Theme;
            cfg.Notifications = dto.Notifications;
            cfg.UpdatedAt = DateTime.UtcNow;
            _db.Configurations.Update(cfg);
            await _db.SaveChangesAsync();
            return new SettingsDto(cfg.Theme, cfg.Notifications);
        }
    }
}
