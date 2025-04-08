namespace BPV_app.Models
{
    public class BPVApproval
    {
        public int Id { get; set; }

        public int BPVProcessId { get; set; }
        public BPVProcess BPVProcess { get; set; }

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }

        public string Status { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewedAt { get; set; }
    }

}
