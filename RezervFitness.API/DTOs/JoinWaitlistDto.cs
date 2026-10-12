namespace RezervFitness.API.DTOs
{
    public class JoinWaitlistDto
    {
        public Guid UserId { get; set; }

        public Guid TimetableScheduleId { get; set; }

        public Guid PackageId { get; set; }

    }
}
