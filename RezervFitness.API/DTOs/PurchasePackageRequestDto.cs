using System.ComponentModel.DataAnnotations;

namespace RezervFitness.API.DTOs
{
    public class PurchasePackageRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int BusinessId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Credits { get; set; }

        [Range(1, 365)]
        public int ValidityDays { get; set; } = 30;
    }
}
