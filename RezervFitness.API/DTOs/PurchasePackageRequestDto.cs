using System.ComponentModel.DataAnnotations;

namespace RezervFitness.API.DTOs
{
    public class PurchasePackageRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BusinessId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Credits { get; set; }

        [Range(1, 365)]
        public int ValidityDays { get; set; } = 30;
    }
}
