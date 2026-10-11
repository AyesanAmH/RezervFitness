using System.ComponentModel.DataAnnotations;

namespace RezervFitness.API.DTOs
{
    public class CreateBookingRequestDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int TimetableScheduleId { get; set; }

        [Range(1, int.MaxValue)]
        public int PackageId { get; set; }
    }
}
