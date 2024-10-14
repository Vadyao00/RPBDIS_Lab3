using Data.Lab3;
using Lab3.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedEventsService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<Event>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Event> events = GetAllFromDb();
            if (events != null)
            {
                _memoryCache.Set(key, events, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Event> GetAll()
        {
            return _dbContext.Events.ToList();
        }

        public IEnumerable<Event> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Events.Take(rowsNumber).ToList();
        }

        public IEnumerable<Event> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Event> events))
            {
                events = GetAllFromDb();
                if (events != null)
                {
                    _memoryCache.Set(cacheKey, events, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return events;
        }
    }
}
