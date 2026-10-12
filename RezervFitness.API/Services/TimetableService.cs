using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RezervFitness.API.DAO;
using RezervFitness.API.Data;
using RezervFitness.API.DTOs;
using RezervFitness.API.Models;
using System.Text.Json;

namespace RezervFitness.API.Services
{
    public class TimetableService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        private readonly TimetableCacheService _timetableCache;

        public TimetableService(
            AppDbContext context,
            IDistributedCache cache,
            TimetableCacheService timetableCache)
        {
            _context = context;
            _cache = cache;
            _timetableCache = timetableCache;
        }

        public async Task<List<TimetableResponseDto>> GetTimetableAsync(
            Guid? businessId,
            DateTime? date)
        {
            var businessKey = businessId?.ToString() ?? "all";
            var dateKey = date?.ToString("yyyy-MM-dd") ?? "all";


            var version = await _timetableCache.GetVersionAsync();

            var cacheKey =
                $"timetable:v{version}:{businessKey}:{dateKey}";

            // 1. Try Redis first
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<TimetableResponseDto>>(
                    cachedData) ?? new List<TimetableResponseDto>();
            }

            // 2. Query MySQL
            var query = _context.TimetableSchedules
                .AsNoTracking()
                .AsQueryable();

            if (businessId.HasValue)
            {
                query = query.Where(s =>
                    s.BusinessId == businessId.Value);
            }

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);

                query = query.Where(s =>
                    s.StartTime >= startOfDay &&
                    s.StartTime < endOfDay);
            }

            var schedules = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new
                {
                    Schedule = s,
                    BusinessName = s.Business.Name,
                    AttendanceCount = _context.Bookings.Count(b =>
                        b.TimetableScheduleId == s.Id &&
                        b.Status == BookingStatus.Confirmed)
                })
                .ToListAsync();

            // 3. Map to your existing DTO
            var result = schedules.Select(x =>
                new TimetableResponseDto
                {
                    ScheduleId = x.Schedule.Id,
                    ClassName = x.Schedule.ClassName,
                    InstructorName = x.Schedule.InstructorName,
                    StartTime = x.Schedule.StartTime,
                    EndTime = x.Schedule.EndTime,

                    AttendanceCount = x.AttendanceCount,

                    AvailableSlots = x.Schedule.AvailableSlots,

                    RemainingSlots = Math.Max(
                        0,
                        x.Schedule.AvailableSlots -
                        x.AttendanceCount),

                    BusinessId = x.Schedule.BusinessId,
                    BusinessName = x.BusinessName
                }).ToList();

            // 4. Cache the result in Redis for 60 seconds
            var json = JsonSerializer.Serialize(result);

            await _cache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromSeconds(60)
                });

            return result;
        }
    }
}


    
