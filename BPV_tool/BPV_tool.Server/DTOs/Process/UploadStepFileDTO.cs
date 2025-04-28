using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.DTOs.Process
{
    public class UploadStepFileDTO
    {
        [Required] 
        public IFormFile File { get; set; }
    }
}
