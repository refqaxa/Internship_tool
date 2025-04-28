namespace BPV_tool.Server.DTOs.Users
{
    public class UserSummaryDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
