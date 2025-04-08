using System.Data;

namespace BPV_app.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<File> UploadedFiles { get; set; }
        public ICollection<Feedback> GivenFeedback { get; set; }
        public ICollection<BPVProcess> SupervisedProcesses { get; set; }
        public ICollection<BPVProcess> StartedProcesses { get; set; }
        public ICollection<BPVApproval> BPVApprovals { get; set; }
        public ICollection<Log> Logs { get; set; }
    }
}
