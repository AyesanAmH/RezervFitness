using Microsoft.EntityFrameworkCore;
using RezervFitness.API.DAO;
using RezervFitness.API.Data;
using RezervFitness.API.DTOs;
using RezervFitness.API.Models;

namespace RezervFitness.API.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;
        private readonly BookingDao _bookingDao;

        private readonly WaitlistService _waitlistService;

        private readonly TimetableCacheService _timetableCache;

        public BookingService(
            AppDbContext context,
            BookingDao bookingDao,
            WaitlistService waitlistService,
            TimetableCacheService timetableCache)
        {
            _context = context;
            _bookingDao = bookingDao;
            _waitlistService = waitlistService;
            _timetableCache = timetableCache;
        }

        public async Task<BookingResponseDto> BookAsync(
            CreateBookingDto request)
        {
            if (request.UserId <= 0 ||
                request.TimetableScheduleId <= 0 ||
                request.PackageId <= 0)
            {
                throw new InvalidOperationException(
                    "User, schedule and package IDs must be valid.");
            }

            // All validation, credit deduction and booking creation
            // happen within one transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock the schedule first so concurrent attempts for
                // the same class are serialized.
                var schedule =
                    await _bookingDao.GetScheduleForUpdateAsync(
                        request.TimetableScheduleId);

                if (schedule == null)
                {
                    throw new InvalidOperationException(
                        "Timetable schedule not found.");
                }

                // Lock the package before checking and changing credits.
                var package =
                    await _bookingDao.GetPackageForUpdateAsync(
                        request.PackageId);

                if (package == null)
                {
                    throw new InvalidOperationException(
                        "Package not found.");
                }

                if (package.UserId != request.UserId)
                {
                    throw new InvalidOperationException(
                        "This package does not belong to the customer.");
                }

                if (package.BusinessId != schedule.BusinessId)
                {
                    throw new InvalidOperationException(
                        "Package and timetable must belong to the same business.");
                }
                if (package.ExpiryDate <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException(
                        "This package has expired.");
                }

                if (package.RemainingCredits < 1)
                {
                    throw new InvalidOperationException(
                        "Insufficient package credits.");
                }

                if (schedule.StartTime <= DateTime.Now)
                {
                    throw new InvalidOperationException(
                        "This class has already started or is in the past.");
                }

                if (schedule.EndTime <= schedule.StartTime)
                {
                    throw new InvalidOperationException(
                        "The timetable schedule has invalid class times.");
                }

                if (await _bookingDao.HasActiveBookingAsync(
                    request.UserId,
                    schedule.Id))
                {
                    throw new InvalidOperationException(
                        "The customer already has an active booking for this class.");
                }

                if (await _bookingDao.HasOverlappingBookingAsync(
                    request.UserId,
                    schedule.StartTime,
                    schedule.EndTime))
                {
                    throw new InvalidOperationException(
                        "The customer already has an overlapping booking.");
                }

                var attendance =
                    await _bookingDao.GetAttendanceCountAsync(
                        schedule.Id);

                if (schedule.AvailableSlots <= 0 ||
                    attendance >= schedule.AvailableSlots)
                {
                    throw new InvalidOperationException(
                        "This class is full. Join the waitlist instead.");
                }

                // Deduct the credit and add the booking in the same
                // transaction.
                package.RemainingCredits--;

                var booking = new Booking
                {
                    UserId = request.UserId,
                    TimetableScheduleId = schedule.Id,
                    PackageId = package.Id,
                    Status = BookingStatus.Confirmed,
                    BookedAt = DateTime.UtcNow,
                    CreditRefunded = false
                };

                await _bookingDao.AddBookingAsync(booking);

                await _context.SaveChangesAsync();

                var updatedAttendance = attendance + 1;

                await transaction.CommitAsync();

                // Invalidate timetable cache after successful booking.
                await _timetableCache.InvalidateAllAsync();

                return new BookingResponseDto
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    TimetableScheduleId = booking.TimetableScheduleId,
                    PackageId = booking.PackageId,
                    Status = booking.Status.ToString(),
                    RemainingCredits = package.RemainingCredits,
                    AttendanceCount = updatedAttendance,
                    AvailablePlaces =
                        schedule.AvailableSlots - updatedAttendance
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> CancelAsync(CancelBookingDto request)
        {
            await using var transaction =
        await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lock the schedule first, using the same lock order
                // as the booking and waitlist promotion workflows.
                var schedules = await _context.TimetableSchedules
                    .FromSqlInterpolated($@"
                SELECT *
                FROM `TimetableSchedules`
                WHERE `Id` = (
                    SELECT `TimetableScheduleId`
                    FROM `Bookings`
                    WHERE `Id` = {request.BookingId}
                )
                FOR UPDATE")
                    .ToListAsync();

                var schedule = schedules.SingleOrDefault();
                if (schedule == null)
                    throw new InvalidOperationException(
                        "Booking or timetable schedule not found.");

                // 2. Lock and load the booking and its package.
                var bookings = await _context.Bookings
                    .FromSqlInterpolated($@"
                SELECT *
                FROM `Bookings`
                WHERE `Id` = {request.BookingId}
                FOR UPDATE")
                    .Include(b => b.Package)
                    .ToListAsync();

                var booking = bookings.SingleOrDefault();

                if (booking == null)
                    throw new InvalidOperationException("Booking not found.");

                if (booking.UserId != request.UserId)
                    throw new InvalidOperationException(
                        "This booking does not belong to the customer.");

                if (booking.TimetableScheduleId != schedule.Id)
                    throw new InvalidOperationException(
                        "Booking schedule mismatch.");

                if (booking.Status != BookingStatus.Confirmed)
                    throw new InvalidOperationException(
                        "This booking is not active.");

                var now = DateTime.UtcNow;

                if (schedule.StartTime <= now)
                    throw new InvalidOperationException(
                        "A class that has started cannot be cancelled.");

                // 3. Apply the four-hour refund rule.
                var refunded = false;

                if (now < schedule.StartTime.AddHours(-4)
                    && !booking.CreditRefunded)
                {
                    booking.Package.RemainingCredits++;
                    booking.CreditRefunded = true;
                    refunded = true;
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = now;

                await _context.SaveChangesAsync();

                // 4. Promote the first eligible waitlist entry.
                // This method must use the same transaction and DbContext.
                var promoted = await _waitlistService.PromoteNextAsync(
                    schedule.Id);

                // 5. Commit both cancellation and promotion atomically.
                await transaction.CommitAsync();

                // 6. Invalidate the cache only after the database commit.
                try
                {
                    await _timetableCache.InvalidateAllAsync();
                }
                catch (Exception)
                {
                    // Cache failure must not turn a committed cancellation
                    // into an apparent failed cancellation.
                    // Cached results will expire after their TTL.
                }

                return refunded
                    ? promoted
                        ? "Booking cancelled, credit refunded, and waitlisted customer promoted."
                        : "Booking cancelled and credit refunded."
                    : promoted
                        ? "Booking cancelled without refund; waitlisted customer promoted."
                        : "Booking cancelled without refund.";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
