namespace BPV_app.Models
{
    public class Log
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public AppUser Student { get; set; }

        public Guid BPVProcessId { get; set; }
        public BPVProcess BPVProcess { get; set; }

        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public string Activity { get; set; }
    }


}
