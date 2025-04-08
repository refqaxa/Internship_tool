namespace BPV_app.Models
{
    public class File
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public AppUser Student { get; set; }

        public string FilePath { get; set; }
        public string BpvStep { get; set; }
        public DateTime UploadedAt { get; set; }

        public ICollection<Feedback> Feedback { get; set; } = new List<Feedback>();
    }


}
