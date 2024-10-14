using Data.Lab3;
using Microsoft.Extensions.Caching.Memory;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab3.Services
{
    public class CachedActorsService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<Actor>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Actor> actors = GetAllFromDb();
            if (actors != null)
            {
                _memoryCache.Set(key, actors, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Actor> GetAll()
        {
            return _dbContext.Actors.Include(a => a.Movies).ToList();
        }

        public IEnumerable<Actor> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Actors.Take(rowsNumber).ToList();
        }

        public IEnumerable<Actor> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Actor> actors))
            {
                actors = GetAllFromDb();
                if (actors != null)
                {
                    _memoryCache.Set(cacheKey, actors, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return actors;
        }
    }
}
