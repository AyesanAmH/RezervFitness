namespace RezervFitness.API.DTOs
{
    public class PackageResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int BusinessId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int TotalCredits { get; set; }

        public int RemainingCredits { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime PurchasedAt { get; set; }
    }
}
