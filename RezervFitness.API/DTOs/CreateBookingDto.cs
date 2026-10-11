namespace RezervFitness.API.DTOs
{
    public class CreateBookingDto
    {
        public int UserId { get; set; }

        public int TimetableScheduleId { get; set; }

        public int PackageId { get; set; }

    }
}
