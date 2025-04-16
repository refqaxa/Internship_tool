using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BPV_app.Data;
using BPV_app.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections;

namespace BPV_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/AppUsers/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.RoleName
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
