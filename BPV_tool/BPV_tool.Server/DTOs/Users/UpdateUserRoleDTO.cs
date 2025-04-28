using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Users
{
    public class UpdateUserRoleDTO
    {
        [Required]
        public Guid NewRoleId { get; set; }
    }
}
