using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace RezervFitness.API.Services
{
    public class TimetableCacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public TimetableCacheService(
            IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<long> GetVersionAsync()
        {
            var db = _redis.GetDatabase();

            var value = await db.StringGetAsync(
                "RezervFitness:timetable:version");

            return value.IsNullOrEmpty ? 0 : (long)value;
        }

        public async Task InvalidateAllAsync()
        {
            var db = _redis.GetDatabase();

            await db.StringIncrementAsync(
                "RezervFitness:timetable:version");
        }

    }
}

