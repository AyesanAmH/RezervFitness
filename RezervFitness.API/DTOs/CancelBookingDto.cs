namespace RezervFitness.API.DTOs
{
    public class CancelBookingDto
    {
        public Guid BookingId { get; set; }

        public Guid UserId { get; set; }
    }
}
