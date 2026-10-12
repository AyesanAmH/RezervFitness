using RezervFitness.API.Data;
using RezervFitness.API.Models;
using Microsoft.EntityFrameworkCore;
namespace RezervFitness.API.DAO
{
    public class PackageDao
    {
        private readonly AppDbContext _context;

        public PackageDao(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Package>> GetAvailablePackagesAsync(
            Guid userId,
            DateTime now)
        {
            return await _context.Packages
                .AsNoTracking()
                .Include(p => p.Business)
                .Where(p =>
                    p.UserId == userId &&
                    p.ExpiryDate > now &&
                    p.RemainingCredits > 0)
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == userId);
        }

        public async Task<bool> BusinessExistsAsync(Guid businessId)
        {
            return await _context.Businesses
                .AnyAsync(b => b.Id == businessId && b.IsActive);
        }

        public async Task<Package> CreatePackageAsync(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return package;
        }


        public async Task<string> GetBusinessNameAsync(Guid businessId)
        {
            return await _context.Businesses
                .Where(b => b.Id == businessId)
                .Select(b => b.Name)
                .SingleAsync();
        }
    }
}
