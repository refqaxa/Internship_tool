using BPV_tool.Server.DTOs.Users;

namespace BPV_tool.Server.DTOs.Users
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public UserSummaryDTO User { get; set; }
    }
}
