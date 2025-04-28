using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Users
{
    public class CreateUserDTO
    {
        [Required]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}
