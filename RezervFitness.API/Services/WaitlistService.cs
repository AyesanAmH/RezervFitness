using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RezervFitness.API.Data;
using RezervFitness.API.Models;

namespace RezervFitness.API.Services
{
    public class WaitlistService
    {
        private readonly AppDbContext _context;

        public WaitlistService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> PromoteNextAsync(int scheduleId)
        {
            // Reuse the caller's transaction when called from cancellation.
            // Otherwise, create a transaction for standalone promotion.
            IDbContextTransaction? ownedTransaction = null;

            if (_context.Database.CurrentTransaction == null)
            {
                ownedTransaction =
                    await _context.Database.BeginTransactionAsync();
            }

            try
            {
                // Always lock the schedule first.
                var schedules = await _context.TimetableSchedules
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM `TimetableSchedules`
                        WHERE `Id` = {scheduleId}
                        FOR UPDATE")
                    .ToListAsync();

                var schedule = schedules.SingleOrDefault();

                if (schedule == null ||
                    schedule.StartTime <= DateTime.UtcNow ||
                    schedule.EndTime <= schedule.StartTime)
                {
                    if (ownedTransaction != null)
                        await ownedTransaction.CommitAsync();

                    return false;
                }

                var attendance = await _context.Bookings
                    .CountAsync(b =>
                        b.TimetableScheduleId == scheduleId &&
                        b.Status == BookingStatus.Confirmed);

                if (attendance >= schedule.AvailableSlots)
                {
                    if (ownedTransaction != null)
                        await ownedTransaction.CommitAsync();

                    return false;
                }

                // FIFO: earliest JoinedAt first, then lowest ID.
                var candidates = await _context.WaitlistEntries
                    .Where(w =>
                        w.TimetableScheduleId == scheduleId &&
                        w.Status == WaitlistStatus.Waiting)
                    .OrderBy(w => w.JoinedAt)
                    .ThenBy(w => w.Id)
                    .ToListAsync();

                foreach (var entry in candidates)
                {
                    // Lock the package before checking or deducting credits.
                    var packages = await _context.Packages
                        .FromSqlInterpolated($@"
                            SELECT *
                            FROM `Packages`
                            WHERE `Id` = {entry.PackageId}
                            FOR UPDATE")
                        .ToListAsync();

                    var package = packages.SingleOrDefault();

                    if (package == null)
                    {
                        entry.Status = WaitlistStatus.Cancelled;
                        continue;
                    }

                    if (package.ExpiryDate <= DateTime.UtcNow)
                    {
                        entry.Status = WaitlistStatus.Expired;
                        continue;
                    }

                    // Invalid or unusable packages are skipped without
                    // deducting credits.
                    if (package.UserId != entry.UserId ||
                        package.BusinessId != schedule.BusinessId ||
                        package.RemainingCredits < 1)
                    {
                        entry.Status = WaitlistStatus.Cancelled;
                        continue;
                    }

                    // Do not create a duplicate active booking.
                    var alreadyBooked = await _context.Bookings
                        .AnyAsync(b =>
                            b.UserId == entry.UserId &&
                            b.TimetableScheduleId == scheduleId &&
                            b.Status == BookingStatus.Confirmed);
                    if (alreadyBooked)
                    {
                        entry.Status = WaitlistStatus.Cancelled;
                        continue;
                    }

                    // Reject promotion when the customer has an
                    // overlapping confirmed booking.
                    var overlaps = await _context.Bookings
                        .AnyAsync(b =>
                            b.UserId == entry.UserId &&
                            b.Status == BookingStatus.Confirmed &&
                            b.TimetableScheduleId != scheduleId &&
                            b.TimetableSchedule.StartTime < schedule.EndTime &&
                            b.TimetableSchedule.EndTime > schedule.StartTime);

                    if (overlaps)
                    {
                        entry.Status = WaitlistStatus.Cancelled;
                        continue;
                    }
                    // Promote exactly one eligible customer.
                    package.RemainingCredits--;

                    var booking = new Booking
                    {
                        UserId = entry.UserId,
                        TimetableScheduleId = scheduleId,
                        PackageId = package.Id,
                        Status = BookingStatus.Confirmed,
                        BookedAt = DateTime.UtcNow,
                        CreditRefunded = false
                    };

                    entry.Status = WaitlistStatus.Promoted;

                    _context.Bookings.Add(booking);

                    await _context.SaveChangesAsync();

                    if (ownedTransaction != null)
                        await ownedTransaction.CommitAsync();

                    return true;
                }

                // Persist statuses for ineligible waitlist entries.
                await _context.SaveChangesAsync();

                if (ownedTransaction != null)
                    await ownedTransaction.CommitAsync();

                return false;
            }
            catch
            {
                if (ownedTransaction != null)
                    await ownedTransaction.RollbackAsync();

                throw;
            }
            finally
            {
                if (ownedTransaction != null)
                    await ownedTransaction.DisposeAsync();
            }
        }

        public async Task<int> ExpireEndedWaitlistsAsync()
        {
            // Expire waiting entries only after their class has ended.
            // This operation never deducts or refunds credits.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;

                var endedEntries = await _context.WaitlistEntries
                    .Where(w =>
                        w.Status == WaitlistStatus.Waiting &&
                        w.TimetableSchedule.EndTime <= now)
                    .ToListAsync();

                foreach (var entry in endedEntries)
                {
                    entry.Status = WaitlistStatus.Expired;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return endedEntries.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

            
