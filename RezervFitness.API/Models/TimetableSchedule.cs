namespace RezervFitness.API.Models
{
    public class TimetableSchedule
    {
        public int Id { get; set; }

        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public string ClassName { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int AvailableSlots { get; set; }
    }
}
