namespace BPV_app.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public int FileId { get; set; }
        public File File { get; set; }

        public int ReviewerId { get; set; }
        public User Reviewer { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
