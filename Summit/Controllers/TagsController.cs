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
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TagsController(AppDbContext db) => _db = db;
        private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IEnumerable<TagResponseDto>> List()
        {
            var uid = CurrentUserId();
            return await _db.Tags.Where(t => t.UserId == uid)
                .Select(t => new TagResponseDto(t.Id, t.Name))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagCreateDto dto)
        {
            var uid = CurrentUserId();
            var tag = new Tag { UserId = uid, Name = dto.Name.Trim() };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(List), new { }, new TagResponseDto(tag.Id, tag.Name));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TagUpdateDto dto)
        {
            var uid = CurrentUserId();
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == uid);
            if (tag == null) return NotFound();
            tag.Name = dto.Name.Trim();
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = CurrentUserId();
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == uid);
            if (tag == null) return NotFound();
            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
