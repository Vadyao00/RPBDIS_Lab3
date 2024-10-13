using Data.Lab3;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedMoviesService(CinemaContext dbContext, IMemoryCache memoryCache) : ICachedService<Movie>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Movie> movies = GetAllFromDb();
            if (movies != null)
            {
                _memoryCache.Set(key, movies, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Movie> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Movies.Include(m => m.Genre).Take(rowsNumber).ToList();
        }

        public IEnumerable<Movie> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Movie> movies))
            {
                movies = GetAllFromDb();
                if (movies != null)
                {
                    _memoryCache.Set(cacheKey, movies, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return movies;
        }
    }
}
