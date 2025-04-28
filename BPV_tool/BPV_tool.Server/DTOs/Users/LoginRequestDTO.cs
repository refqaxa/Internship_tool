using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Users
{
    public class LoginRequestDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
