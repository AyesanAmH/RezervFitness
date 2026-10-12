namespace RezervFitness.API.DTOs
{
    public class BookingResponseDto
    {
        public Guid BookingId { get; set; }

        public Guid UserId { get; set; }

        public Guid TimetableScheduleId { get; set; }

        public Guid PackageId { get; set; }

        public string Status { get; set; } = string.Empty;

        public int RemainingCredits { get; set; }

        public int AttendanceCount { get; set; }

        public int AvailablePlaces { get; set; }
    }
}
