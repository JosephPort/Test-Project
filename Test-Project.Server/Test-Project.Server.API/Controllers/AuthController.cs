using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test_Project.Server.API.Context;
using Test_Project.Server.Models.DTOs;
using Test_Project.Server.Models.Tables;

namespace Test_Project.Server.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EfContext _context;

        public AuthController(EfContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return BadRequest("Invalid username or password.");
            }

            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO request)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (dbUser is not null)
            {
                return BadRequest("User already exists with this username or email.");
            }

            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
