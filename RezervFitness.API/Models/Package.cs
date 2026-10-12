namespace RezervFitness.API.Models
{
    public class Package
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        public int TotalCredits { get; set; }

        public int RemainingCredits { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}
