namespace RezervFitness.API.Models
{
    public enum BookingStatus
    {
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }

    public class Booking
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid TimetableScheduleId { get; set; }
        public TimetableSchedule TimetableSchedule { get; set; } = null!;

        public Guid PackageId { get; set; }
        public Package Package { get; set; } = null!;

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CancelledAt { get; set; }

        public bool CreditRefunded { get; set; }
    }
}
