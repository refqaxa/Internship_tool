using System.Data;

namespace BPV_app.Models
{
    public class AppUser
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public Guid RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<File> UploadedFiles { get; set; } = new List<File>();
        public ICollection<Feedback> GivenFeedback { get; set; } = new List<Feedback>();
        public ICollection<BPVProcess> SupervisedProcesses { get; set; } = new List<BPVProcess>();
        public ICollection<BPVProcess> StartedProcesses { get; set; } = new List<BPVProcess>();
        public ICollection<BPVApproval> BPVApprovals { get; set; } = new List<BPVApproval>();
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }

}
