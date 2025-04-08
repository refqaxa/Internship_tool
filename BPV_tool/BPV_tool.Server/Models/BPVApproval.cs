namespace BPV_app.Models
{
    public class BPVApproval
    {
        public Guid Id { get; set; }

        public Guid BPVProcessId { get; set; }
        public BPVProcess BPVProcess { get; set; }

        public Guid ReviewerId { get; set; }
        public AppUser Reviewer { get; set; }

        public string Status { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewedAt { get; set; }
    }


}
