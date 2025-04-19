using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using Microsoft.AspNetCore.Authorization;
using BPV_tool.Server.DTOs.Users;

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
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

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

            var response = new LoginResponseDTO
            {
                Token = jwt,
                User = new UserSummaryDTO
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role.RoleName
                }
            };

            return Ok(response);
        }

        // GET: api/AppUsers/Roles
        [HttpGet("Roles")]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleDTO
                {
                    Id = r.Id,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/AppUsers/allUsers
        [HttpGet("AllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserSummaryDTO>>> GetUsersWithRoles()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new UserSummaryDTO
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    Role = u.Role.RoleName
                }).ToListAsync();

            return Ok(users);
        }

        // POST: api/AppUsers/CreateUser
        [HttpPost("CreateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
            if (role == null)
                return BadRequest("Invalid role.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            var newUser = new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId
                // Role = role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/AppUsers/UpdateRole/{id}
        [HttpPut("UpdateRole/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var role = await _context.Roles.FindAsync(dto.NewRoleId);
            if (role == null)
                return BadRequest("Invalid role ID.");

            user.RoleId = dto.NewRoleId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/AppUsers/DeleteAppUser/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAppUser(Guid id)
        {
            var appUser = await _context.Users.FindAsync(id);
            if (appUser == null)
                return NotFound();

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
