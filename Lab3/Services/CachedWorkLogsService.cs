using Data.Lab3;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedWorkLogsService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<WorkLog>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<WorkLog> workLogs = GetAllFromDb();
            if (workLogs != null)
            {
                _memoryCache.Set(key, workLogs, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<WorkLog> GetAll()
        {
            return _dbContext.WorkLogs.ToList();
        }

        public IEnumerable<WorkLog> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.WorkLogs.Include(w => w.Employee).Take(rowsNumber).ToList();
        }

        public IEnumerable<WorkLog> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<WorkLog> workLogs))
            {
                workLogs = GetAllFromDb();
                if (workLogs != null)
                {
                    _memoryCache.Set(cacheKey, workLogs, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return workLogs;
        }
    }
}
