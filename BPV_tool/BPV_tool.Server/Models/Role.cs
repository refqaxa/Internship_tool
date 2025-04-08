namespace BPV_app.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }


}
