using Data.Lab3;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedSeatsService(CinemaContext dbContext, IMemoryCache memoryCache) : ICachedService<Seat>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Seat> seats = GetAllFromDb();
            if (seats != null)
            {
                _memoryCache.Set(key, seats, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Seat> GetAll()
        {
            return _dbContext.Seats.Include(s => s.Event).Include(s => s.Showtime).ThenInclude(s => s.Movie).ToList();
        }

        public IEnumerable<Seat> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Seats.Include(s => s.Event).Include(s => s.Showtime).ThenInclude(s => s.Movie).Take(rowsNumber).ToList();
        }

        public IEnumerable<Seat> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Seat> seats))
            {
                seats = GetAllFromDb();
                if (seats != null)
                {
                    _memoryCache.Set(cacheKey, seats, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return seats;
        }
    }
}
