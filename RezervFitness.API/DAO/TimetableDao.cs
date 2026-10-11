using RezervFitness.API.Data;
using RezervFitness.API.Models;
using Microsoft.EntityFrameworkCore;

namespace RezervFitness.API.DAO
{
    public class TimetableDao
    {
        private readonly AppDbContext _context;

        public TimetableDao(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimetableSchedule>> GetSchedulesAsync(
            int? businessId,
            DateTime? date)
        {
            var query = _context.TimetableSchedules
                .AsNoTracking()
                .Include(s => s.Business)
                .AsQueryable();

            if (businessId.HasValue)
            {
                query = query.Where(s =>
                    s.BusinessId == businessId.Value);
            }

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);

                query = query.Where(s =>
                    s.StartTime >= start && s.StartTime < end);
            }

            return await query
            .OrderBy(s => s.StartTime)
            .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetAttendanceCountsAsync(
        List<int> scheduleIds)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    scheduleIds.Contains(b.TimetableScheduleId) &&
                    b.Status == BookingStatus.Confirmed)
                .GroupBy(b => b.TimetableScheduleId)
                .Select(g => new
                {
                    ScheduleId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.ScheduleId,
                    x => x.Count);
        }
    }

}



