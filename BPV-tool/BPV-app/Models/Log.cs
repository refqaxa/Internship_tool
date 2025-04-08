namespace BPV_app.Models
{
    public class Log
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public int BPVProcessId { get; set; }
        public BPVProcess BPVProcess { get; set; }

        public DateTime Date { get; set; }
        public int Hours { get; set; }
        public string Activity { get; set; }
    }

}
