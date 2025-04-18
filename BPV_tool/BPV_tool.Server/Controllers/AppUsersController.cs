using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using Microsoft.AspNetCore.Authorization;

namespace BPV_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AppUsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/AppUsers/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            // JWT Key from config
            var key = _configuration["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
            }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = jwt,
                user = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    Role = user.Role.RoleName
                }
            });
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // GET: api/AppUsers/Roles
        [HttpGet("Roles")]
        public async Task<ActionResult<IEnumerable<object>>> CreateUser()
        {
            var roles = await _context.Roles.ToListAsync();

            return Ok(roles);
        }

        // GET: api/AppUsers/allUsers
        [HttpGet("AllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersWithRoles()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new {
                    u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    u.Email,
                    Role = u.Role.RoleName
                }).ToListAsync();

            return Ok(users);
        }

        // POST: api/AppUsers/CreateUser
        [HttpPost("CreateUser")]
        [Authorize(Roles = "Admin")]
        // Task: Instead of binding the entire AppUser model directly in your controller, create a CreateUserDTO to explicitly define the expected input. This prevents over-posting and reduces bugs.
        public async Task<IActionResult> CreateUser(AppUser newUser)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == newUser.RoleId);
            if (role == null) return BadRequest("Invalid role.");

            // Check for existing email
            if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
                return BadRequest("Email already exists.");

            newUser.Id = Guid.NewGuid();
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);
            _context.Users.Add(newUser); try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }


        // PUT: api/AppUsers/UpdateRole/{id}
        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] Guid newRoleId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found.");

            var role = await _context.Roles.FindAsync(newRoleId);
            if (role == null) return BadRequest("Invalid role ID.");

            user.RoleId = newRoleId; try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/DeleteUser/
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAppUser(Guid id)
        {
            var appUser = await _context.Users.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool AppUserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
