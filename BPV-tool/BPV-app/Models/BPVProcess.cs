namespace BPV_app.Models
{
    public class BPVProcess
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public string CompanyName { get; set; }

        public int SupervisorId { get; set; }
        public User Supervisor { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<BPVApproval> Approvals { get; set; }
        public ICollection<Log> Logs { get; set; }
    }

}
