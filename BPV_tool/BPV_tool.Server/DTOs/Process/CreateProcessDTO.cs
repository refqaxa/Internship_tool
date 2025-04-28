using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Process
{
    public class CreateProcessDTO
    {
        [Required] 
        public string CompanyName { get; set; }
        [Required] 
        public Guid SupervisorId { get; set; }
    }
}
