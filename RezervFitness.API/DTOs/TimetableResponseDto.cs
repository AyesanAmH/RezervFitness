namespace RezervFitness.API.DTOs
{
    public class TimetableResponseDto
    {
        public Guid ScheduleId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int AttendanceCount { get; set; }

        public int AvailableSlots { get; set; }

        public int RemainingSlots { get; set; }

        public Guid BusinessId { get; set; }

        public string BusinessName { get; set; } = string.Empty;
    }
}
