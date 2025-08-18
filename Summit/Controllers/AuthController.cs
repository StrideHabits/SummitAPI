using System.Security.Claims;
using Summit.Data;
using Summit.DTOs;
using Summit.Helpers;
using Summit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Summit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtHelper _jwt;

        public AuthController(AppDbContext db, JwtHelper jwt)
        {
            _db = db; _jwt = jwt;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password required.");

            if (_db.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already taken.");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized();

            var token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _db.Users.Where(u => u.Id == uid)
                                .Select(u => new { u.Id, u.Username })
                                .FirstOrDefault();
            return Ok(user);
        }
    }
}
