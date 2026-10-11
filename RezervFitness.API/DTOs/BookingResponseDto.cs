namespace RezervFitness.API.DTOs
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int TimetableScheduleId { get; set; }

        public int PackageId { get; set; }

        public string Status { get; set; } = string.Empty;

        public int RemainingCredits { get; set; }

        public int AttendanceCount { get; set; }

        public int AvailablePlaces { get; set; }
    }
}
