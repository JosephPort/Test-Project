using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly string _secretKey = "2342342342342342342342342342342342342342342342342342342342342342";

        public AuthController(EfContext context)
        {
            _context = context;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var accessToken = Request.Cookies["Access"];
            var refreshToken = Request.Cookies["Refresh"];

            if (accessToken is null || refreshToken is null)
            {
                return Unauthorized("One or more tokens not provided.");
            }

            var userId = ValidateToken(accessToken);
            if (userId is null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user is null)
            {
                return Unauthorized("User not found.");
            }

            var userDTO = new UserDTO
            {
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                ProfilePicture = user.ProfilePicture ?? string.Empty
            };

            return Ok(userDTO);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return BadRequest("Invalid username or password.");
            }

            var accessToken = CreateToken(user, false);
            var refreshToken = CreateToken(user, true);

            CreateHTTPOnlyCookie(accessToken, false);
            CreateHTTPOnlyCookie(refreshToken, true);

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

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var accessToken = Request.Cookies["Access"];
            var refreshToken = Request.Cookies["Refresh"];

            if (accessToken is null || refreshToken is null)
            {
                return Unauthorized("One or more tokens not provided.");
            }

            var userId = ValidateToken(accessToken);
            if (userId is null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user is null)
            {
                return Unauthorized("User not found.");
            }

            var newAccessToken = CreateToken(user, false);
            CreateHTTPOnlyCookie(newAccessToken, false);

            return Ok();
        }

        private int? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

                return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
            }
            catch
            {
                return null;
            }
        }

        private string CreateToken(User user, bool isRefresh)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = isRefresh ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private void CreateHTTPOnlyCookie(string token, bool isRefresh)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            };

            Response.Cookies.Append(isRefresh ? "Refresh" : "Access", token, cookieOptions);
        }
    }
}