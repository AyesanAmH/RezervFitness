using Microsoft.EntityFrameworkCore;
using RezervFitness.API.Data;
using RezervFitness.API.Models;

namespace RezervFitness.API.DAO
{
    public class BookingDao
    {
        private readonly AppDbContext _context;

        public BookingDao(AppDbContext context)
        {
            _context = context;
        }

        // Lock the schedule row while processing a booking.
        // Requires a MySQL table using InnoDB.
        public async Task<TimetableSchedule?> GetScheduleForUpdateAsync(
            Guid scheduleId)
        {
            var schedules = await _context.TimetableSchedules
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM `TimetableSchedules`
                    WHERE `Id` = {scheduleId}
                    FOR UPDATE")
                .ToListAsync();

            return schedules.SingleOrDefault();
        }

        // Lock the package row before changing its credits.
        public async Task<Package?> GetPackageForUpdateAsync(
            Guid packageId)
        {
            var packages = await _context.Packages
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM `Packages`
                    WHERE `Id` = {packageId}
                    FOR UPDATE")
                .ToListAsync();

            return packages.SingleOrDefault();
        }

        // Count confirmed bookings for a schedule.
        public Task<int> GetAttendanceCountAsync(Guid scheduleId)
        {
            return _context.Bookings.CountAsync(b =>
                b.TimetableScheduleId == scheduleId &&
                b.Status == BookingStatus.Confirmed);
        }

        // Check whether the customer already has an active
        // booking for this exact schedule.
        public Task<bool> HasActiveBookingAsync(
            Guid userId,
            Guid scheduleId)
        {
            return _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.TimetableScheduleId == scheduleId &&
                b.Status == BookingStatus.Confirmed);
        }

        // Find overlapping confirmed bookings.
        public Task<bool> HasOverlappingBookingAsync(
            Guid userId,
            DateTime startTime,
            DateTime endTime)
        {
            return _context.Bookings.AnyAsync(b =>
               b.UserId == userId &&
               b.Status == BookingStatus.Confirmed &&
               b.TimetableSchedule.StartTime < endTime &&
               b.TimetableSchedule.EndTime > startTime);
        }

        // Add a new booking to the current EF Core unit of work.
        public async Task AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }
    }
}

        


