using Data.Lab3;
using Microsoft.Extensions.Caching.Memory;
using Lab3.Models;

namespace Lab3.Services
{
    public class CachedEmployeesService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<Employee>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Employee> employees = GetAllFromDb();
            if (employees != null)
            {
                _memoryCache.Set(key, employees, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Employee> GetAll()
        {
            return _dbContext.Employees.ToList();
        }

        public IEnumerable<Employee> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Employees.Take(rowsNumber).ToList();
        }

        public IEnumerable<Employee> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Employee> employees))
            {
                employees = GetAllFromDb();
                if (employees != null)
                {
                    _memoryCache.Set(cacheKey, employees, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return employees;
        }
    }
}
