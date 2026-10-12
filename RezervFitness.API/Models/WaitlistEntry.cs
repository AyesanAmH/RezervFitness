namespace RezervFitness.API.Models
{
    public enum WaitlistStatus
    {
        Waiting = 1,
        Promoted = 2,
        Expired = 3,
        Cancelled = 4
    }

    public class WaitlistEntry
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid TimetableScheduleId { get; set; }
        public TimetableSchedule TimetableSchedule { get; set; } = null!;

        public Guid PackageId { get; set; }
        public Package Package { get; set; } = null!;

        public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
