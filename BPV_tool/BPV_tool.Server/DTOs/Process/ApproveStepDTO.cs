using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Process
{
    public class ApproveStepDTO
    {
        [Required] 
        public string Status { get; set; }  // "goedgekeurd" or "afgewezen"
        public string? Comment { get; set; }
    }
}
