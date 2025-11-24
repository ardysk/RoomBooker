using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Infrastructure.Data;

namespace RoomBooker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly RoomBookerDbContext _db;

        public UsersController(RoomBookerDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _db.Users
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    Role = u.Role,
                    Password = ""
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role,
                Password = ""
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Email = dto.Email;
            user.DisplayName = dto.DisplayName;
            user.Role = dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}