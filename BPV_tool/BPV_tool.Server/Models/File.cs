using BPV_tool.Server.Models;

namespace BPV_app.Models
{
    public class File
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public AppUser Student { get; set; }

        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }

        public ICollection<BPVStep> ProcessSteps { get; set; } = new List<BPVStep>();
    }


}
