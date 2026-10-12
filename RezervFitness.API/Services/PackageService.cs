using RezervFitness.API.DAO;
using RezervFitness.API.DTOs;

namespace RezervFitness.API.Services
{
    public class PackageService
    {
        private readonly PackageDao _packageDao;

        public PackageService(PackageDao packageDao)
        {
            _packageDao = packageDao;
        }

        public async Task<List<PackageResponseDto>> GetAvailablePackagesAsync(
        Guid userId)
        {
            var now = DateTime.UtcNow;

            var packages = await _packageDao
                .GetAvailablePackagesAsync(userId, now);

            return packages.Select(p => new PackageResponseDto
            {
                Id = p.Id,
                UserId = p.UserId,
                BusinessId = p.BusinessId,
                BusinessName = p.Business.Name,
                TotalCredits = p.TotalCredits,
                RemainingCredits = p.RemainingCredits,
                ExpiryDate = p.ExpiryDate,
                PurchasedAt = p.PurchasedAt
            }).ToList();
        }

        public async Task<PackageResponseDto> PurchasePackageAsync(
    PurchasePackageRequestDto request)
        {
            if (!await _packageDao.UserExistsAsync(request.UserId))
            {
                throw new ArgumentException("User does not exist.");
            }

            if (!await _packageDao.BusinessExistsAsync(request.BusinessId))
            {
                throw new ArgumentException(
                    "Business does not exist or is inactive.");
            }

            var now = DateTime.UtcNow;

            var package = new RezervFitness.API.Models.Package
            {
                UserId = request.UserId,
                BusinessId = request.BusinessId,
                TotalCredits = request.Credits,
                RemainingCredits = request.Credits,
                PurchasedAt = now,
                ExpiryDate = now.AddDays(request.ValidityDays)
            };

            var created = await _packageDao.CreatePackageAsync(package);

            return new PackageResponseDto
            {
                Id = created.Id,
                UserId = created.UserId,
                BusinessId = created.BusinessId,
                BusinessName = (await _packageDao
                    .GetBusinessNameAsync(created.BusinessId)),
                TotalCredits = created.TotalCredits,
                RemainingCredits = created.RemainingCredits,
                ExpiryDate = created.ExpiryDate,
                PurchasedAt = created.PurchasedAt
            };

        }


     }


    }
