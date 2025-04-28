using BPV_app.Models;
using System.ComponentModel.DataAnnotations;

namespace BPV_tool.Server.Models
{
    public class BPVStep
    {
        public Guid Id { get; set; }
        public Guid BPVProcessId { get; set; }
        public BPVProcess BPVProcess { get; set; }

        // Static step info
        public int StepIndex { get; set; }       // 1…4
        public string StepName { get; set; }     // e.g. "BPV‑contract upload"

        // Uploaded file (optional until student uploads)
        public Guid? FileId { get; set; }
        public BPV_app.Models.File File { get; set; }

        // Approval record (optional until teacher reviews)
        public Guid? ApprovalId { get; set; }
        public BPVApproval Approval { get; set; }
    }
}
