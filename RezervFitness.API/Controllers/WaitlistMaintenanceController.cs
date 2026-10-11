using Microsoft.AspNetCore.Mvc;
using RezervFitness.API.Services;

namespace RezervFitness.API.Controllers
{
    [ApiController]
    [Route("api/waitlist/maintenance")]
    public class WaitlistMaintenanceController : ControllerBase
    {
        private readonly WaitlistService _waitlistService;

        public WaitlistMaintenanceController(
            WaitlistService waitlistService)
        {
            _waitlistService = waitlistService;
        }

        [HttpPost("expire-ended")]
        public async Task<IActionResult> ExpireEndedWaitlists()
        {
            var count = await _waitlistService
                .ExpireEndedWaitlistsAsync();

            return Ok(new
            {
                message = "Ended waitlist entries processed.",
                expiredCount = count
            });
        }
    }
}
