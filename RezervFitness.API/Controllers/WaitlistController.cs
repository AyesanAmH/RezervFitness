using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezervFitness.API.Data;
using RezervFitness.API.DTOs;
using RezervFitness.API.Models;

namespace RezervFitness.API.Controllers
{
    [ApiController]
    [Route("api/waitlist")]
    public class WaitlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WaitlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Join(
            [FromBody] JoinWaitlistDto request)
        {
            if (request.UserId <= 0 ||
                request.TimetableScheduleId <= 0 ||
                request.PackageId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid user, schedule and package IDs are required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var schedules = await _context.TimetableSchedules
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM `TimetableSchedules`
                        WHERE `Id` = {request.TimetableScheduleId}
                        FOR UPDATE")
                    .ToListAsync();

                var schedule = schedules.SingleOrDefault();

                if (schedule == null)
                {
                    return NotFound(new
                    {
                        message = "Timetable schedule not found."
                    });

                }

                if (schedule.StartTime <= DateTime.Now)
                {
                    return Conflict(new
                    {
                        message = "Cannot join a waitlist for a class that has started."
                    });
                }

                var packages = await _context.Packages
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM `Packages`
                        WHERE `Id` = {request.PackageId}
                        FOR UPDATE")
                    .ToListAsync();

                var package = packages.SingleOrDefault();

                if (package == null)
                {
                    return NotFound(new
                    {
                        message = "Package not found."
                    });

                }

                if (package.UserId != request.UserId ||
                    package.BusinessId != schedule.BusinessId)
                {
                    return Conflict(new
                    {
                        message = "Package owner or business does not match."
                    });
                }

                if (package.ExpiryDate <= DateTime.UtcNow ||
                    package.RemainingCredits < 1)
                {
                    return Conflict(new
                    {
                        message = "Package is expired or has insufficient credits."
                    });
                }

                var attendance = await _context.Bookings.CountAsync(b =>
                    b.TimetableScheduleId == schedule.Id &&
                    b.Status == BookingStatus.Confirmed);

                if (attendance < schedule.AvailableSlots)
                {
                    return Conflict(new
                    {
                        message = "The class has available places. Book it directly."
                    });
                }

                var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                    b.UserId == request.UserId &&
                    b.TimetableScheduleId == schedule.Id &&
                    b.Status == BookingStatus.Confirmed);

                if (alreadyBooked)
                {
                    return Conflict(new
                    {
                        message = "You already have a booking for this class."
                    });
                }

                var existingEntry = await _context.WaitlistEntries.AnyAsync(w =>
                    w.UserId == request.UserId &&
                    w.TimetableScheduleId == schedule.Id &&
                    w.Status == WaitlistStatus.Waiting);

                if (existingEntry)
                {
                    return Conflict(new
                    {
                        message = "You are already on this waitlist."
                    });
                }

                var entry = new WaitlistEntry
                {
                    UserId = request.UserId,
                    TimetableScheduleId = schedule.Id,
                    PackageId = package.Id,
                    JoinedAt = DateTime.UtcNow,
                    Status = WaitlistStatus.Waiting
                };

                _context.WaitlistEntries.Add(entry);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Successfully joined the waitlist.",
                    waitlistId = entry.Id,
                    entry.UserId,
                    entry.TimetableScheduleId,
                    entry.JoinedAt,
                    status = entry.Status.ToString()
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
        
