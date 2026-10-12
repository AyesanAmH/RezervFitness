using Microsoft.AspNetCore.Mvc;
using RezervFitness.API.DTOs;
using RezervFitness.API.Services;

namespace RezervFitness.API.Controllers
{
    [ApiController]
    [Route("api/packages")]
    public class PackagesController : ControllerBase
    {
        private readonly PackageService _packageService;

        public PackagesController(PackageService packageService)
        {
            _packageService = packageService;
        }

        // GET /api/packages?userId=1
        [HttpGet]
        public async Task<IActionResult> GetPackages(
            [FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "A valid userId is required."
                });
            }

            var packages = await _packageService
           .GetAvailablePackagesAsync(userId);

            return Ok(packages);
        }


        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePackage(
    [FromBody] PurchasePackageRequestDto request)
        {
            try
            {
                var package = await _packageService
                    .PurchasePackageAsync(request);

                return CreatedAtAction(
                    nameof(GetPackages),
                    new { userId = request.UserId },
                    package);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
    
