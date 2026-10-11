using Microsoft.AspNetCore.Mvc;
using RezervFitness.API.DTOs;
using RezervFitness.API.Services;

namespace RezervFitness.API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingsController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(
            [FromBody] CreateBookingDto request)
        {
            try
            {
                var result = await _bookingService.BookAsync(request);

                return Created(
                    $"/api/bookings/{result.BookingId}",
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking(
    [FromBody] CancelBookingDto request)
        {
            try
            {
                var message = await _bookingService.CancelAsync(request);

                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
    
