namespace BPV_app.Models
{
    public class BPVProcess
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public AppUser Student { get; set; }

        public string CompanyName { get; set; }

        public Guid SupervisorId { get; set; }
        public AppUser Supervisor { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<BPVApproval> Approvals { get; set; } = new List<BPVApproval>();
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }


}
