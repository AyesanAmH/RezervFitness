namespace RezervFitness.API.DTOs
{
    public class PackageResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid BusinessId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int TotalCredits { get; set; }

        public int RemainingCredits { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime PurchasedAt { get; set; }
    }
}
