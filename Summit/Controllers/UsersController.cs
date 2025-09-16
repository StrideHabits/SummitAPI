using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummitAPI.Data;
using SummitAPI.Dtos;
using SummitAPI.Models;
using SummitAPI.Service;
using System.Security.Cryptography;
using System.Text;

namespace SummitAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _jwt;
        public UsersController(AppDbContext db, ITokenService jwt)
        {
            _db = db; _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and Password required.");

            var email = req.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = req.Name?.Trim() ?? "",
                Email = email,
                PasswordHash = HashPassword(req.Password)
            };
            _db.Users.Add(user);
            _db.Settings.Add(new UserSettings { Id = Guid.NewGuid(), UserId = user.Id });
            await _db.SaveChangesAsync();

            return Ok(new RegisterResponse(user.Id, user.Email));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        {
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null || user.PasswordHash != HashPassword(req.Password))
                return Unauthorized("Invalid credentials.");

            var token = _jwt.GenerateToken(user.Id, user.Email);
            return new AuthResponse(user.Id, user.Email, token);
        }

        private static string HashPassword(string password)
        {
            var salt = "summitapi_salt_v1"; // MVP; for prod, store per-user salts
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 100_000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }
    }
}
