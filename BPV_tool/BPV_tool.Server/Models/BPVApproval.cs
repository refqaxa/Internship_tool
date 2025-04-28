using BPV_tool.Server.Models;

namespace BPV_app.Models
{
    public class BPVApproval
    {
        public Guid Id { get; set; }
        public Guid? BPVStepId { get; set; }
        public BPVStep? BPVStep { get; set; }
        public Guid? ReviewerId { get; set; }
        public AppUser? Reviewer { get; set; }

        public string Status { get; set; }
        public string? Feedback { get; set; }
        public DateTime ReviewedAt { get; set; }
    }


}
