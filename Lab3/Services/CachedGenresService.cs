using Data.Lab3;
using Lab3.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedGenresService(CinemaContext dbContext, IMemoryCache memoryCache) : ICachedService<Genre>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Genre> genres = GetAllFromDb();
            if (genres != null)
            {
                _memoryCache.Set(key, genres, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Genre> GetAll()
        {
            return _dbContext.Genres.ToList();
        }

        public IEnumerable<Genre> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Genres.Take(rowsNumber).ToList();
        }

        public IEnumerable<Genre> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if(!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Genre> genres))
            {
                genres = GetAllFromDb();
                if(genres != null)
                {
                    _memoryCache.Set(cacheKey, genres, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return genres;
        }
    }
}
