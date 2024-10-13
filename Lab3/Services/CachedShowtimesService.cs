using Data.Lab3;
using Lab3.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Lab3.Services
{
    public class CachedShowtimesService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<Showtime>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Showtime> showtimes = GetAllFromDb();
            if (showtimes != null)
            {
                _memoryCache.Set(key, showtimes, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Showtime> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Showtimes.Include(s => s.Movie).Take(rowsNumber).ToList();
        }

        public IEnumerable<Showtime> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Showtime> showtimes))
            {
                showtimes = GetAllFromDb();
                if (showtimes != null)
                {
                    _memoryCache.Set(cacheKey, showtimes, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return showtimes;
        }
    }
}
