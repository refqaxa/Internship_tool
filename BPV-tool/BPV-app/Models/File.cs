namespace BPV_app.Models
{
    public class File
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public string FilePath { get; set; }
        public string BpvStep { get; set; }
        public DateTime UploadedAt { get; set; }

        public ICollection<Feedback> Feedback { get; set; }
    }

}
