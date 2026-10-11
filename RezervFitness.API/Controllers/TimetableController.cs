using Microsoft.AspNetCore.Mvc;
using RezervFitness.API.Services;

namespace RezervFitness.API.Controllers
{
    [ApiController]
    [Route("api/timetable")]
    public class TimetableController : ControllerBase
    {
        private readonly TimetableService _timetableService;

        private readonly TimetableCacheService _timetableCache;
        public TimetableController(TimetableService timetableService)
        {
            _timetableService = timetableService;
        }



        [HttpGet]
        public async Task<IActionResult> GetTimetable(
        [FromQuery] int? businessId,
        [FromQuery] DateTime? date)
        {
            var schedules = await _timetableService
                .GetTimetableAsync(businessId, date);

            return Ok(schedules);
        }
    }
}
    
