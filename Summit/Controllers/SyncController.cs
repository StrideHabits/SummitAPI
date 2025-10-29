using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummitAPI.Data;
using SummitAPI.Dtos;
using SummitAPI.Models;

namespace SummitAPI.Controllers
{
    [ApiController]
    [Route("sync/checkins")]
    [Authorize] // ensure User claims available
    public class SyncController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SyncController(AppDbContext db) { _db = db; }

        private Guid CurrentUserId()
        {
            // adjust to your token: try uid, sub, or nameidentifier
            var id = User.FindFirst("uid")?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(id!);
        }

        [HttpPost("push")]
        public async Task<ActionResult<List<PushResult>>> Push([FromBody] List<CheckInPushItem> items)
        {
            var userId = CurrentUserId();
            var results = new List<PushResult>();

            using var tx = await _db.Database.BeginTransactionAsync();

            foreach (var i in items)
            {
                // idempotency
                var cached = await _db.RequestLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.RequestId == i.RequestId);
                if (cached != null)
                {
                    var res = System.Text.Json.JsonSerializer.Deserialize<PushResult>(cached.ResultJson)!;
                    results.Add(res);
                    continue;
                }

                try
                {
                    // verify habit belongs to user
                    var habitUser = await _db.Habits
                        .Where(h => h.Id == i.HabitId)
                        .Select(h => h.UserId)
                        .FirstOrDefaultAsync();

                    if (habitUser == Guid.Empty || habitUser != userId)
                        return Forbid();

                    var entity = await _db.HabitCompletions.FirstOrDefaultAsync(x => x.Id == i.Id);
                    if (entity == null)
                    {
                        entity = new HabitCompletion { Id = i.Id, HabitId = i.HabitId };
                        _db.HabitCompletions.Add(entity);
                    }

                    entity.DayKey = i.DayKey;
                    entity.CompletedAt = i.CompletedAt;
                    entity.Deleted = i.Deleted;

                    if (!string.IsNullOrWhiteSpace(i.BaseVersion))
                    {
                        var baseBytes = Convert.FromBase64String(i.BaseVersion);
                        _db.Entry(entity).Property(nameof(HabitCompletion.RowVersion)).OriginalValue = baseBytes;
                    }

                    await _db.SaveChangesAsync(); // sets UpdatedAt; EF updates RowVersion

                    var applied = new PushResult(
                        entity.Id,
                        entity.UpdatedAt,
                        Convert.ToBase64String(entity.RowVersion),
                        "applied",
                        null
                    );
                    results.Add(applied);

                    _db.RequestLogs.Add(new RequestLog
                    {
                        UserId = userId,
                        RequestId = i.RequestId,
                        ResultJson = System.Text.Json.JsonSerializer.Serialize(applied)
                    });
                }
                catch (DbUpdateConcurrencyException)
                {
                    var cur = await _db.HabitCompletions.AsNoTracking().FirstAsync(x => x.Id == i.Id);
                    results.Add(new PushResult(
                        cur.Id,
                        cur.UpdatedAt,
                        Convert.ToBase64String(cur.RowVersion),
                        "conflict",
                        "stale_version"
                    ));
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(results);
        }

        [HttpGet("changes")]
        public async Task<ActionResult<ChangesPage>> Changes([FromQuery] DateTime? since = null, [FromQuery] int pageSize = 200)
        {
            var userId = CurrentUserId();

            var q = from c in _db.HabitCompletions
                    join h in _db.Habits on c.HabitId equals h.Id
                    where h.UserId == userId && (since == null || c.UpdatedAt > since)
                    orderby c.UpdatedAt
                    select new CheckInChange(
                        c.Id,
                        c.HabitId,
                        c.DayKey,
                        c.CompletedAt,
                        c.Deleted,
                        c.UpdatedAt,
                        Convert.ToBase64String(c.RowVersion)
                    );

            var items = await q.Take(pageSize).ToListAsync();
            var hasMore = items.Count == pageSize && await q.Skip(pageSize).AnyAsync();
            var nextSince = items.Count > 0 ? items[^1].UpdatedAt : since;

            return Ok(new ChangesPage(items, hasMore, nextSince));
        }
    }
}
