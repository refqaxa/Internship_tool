namespace BPV_tool.Server.DTOs.Process
{
    public class ProcessSummaryDTO
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
