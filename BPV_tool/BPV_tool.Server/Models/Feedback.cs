namespace BPV_app.Models
{
    public class Feedback
    {
        public Guid Id { get; set; }

        public Guid FileId { get; set; }
        public File File { get; set; }

        public Guid ReviewerId { get; set; }
        public AppUser Reviewer { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
