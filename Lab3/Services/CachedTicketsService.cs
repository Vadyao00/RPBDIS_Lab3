using Data.Lab3;
using Lab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Lab3.Services
{
    public class CachedTicketsService(IMemoryCache memoryCache, CinemaContext dbContext) : ICachedService<Ticket>
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly CinemaContext _dbContext = dbContext;

        public void AddIntoCache(string key, int rowsNumber = 20)
        {
            IEnumerable<Ticket> tickets = GetAllFromDb();
            if (tickets != null)
            {
                _memoryCache.Set(key, tickets, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(266)
                });
            }
        }

        public IEnumerable<Ticket> GetAllFromDb(int rowsNumber = 20)
        {
            return _dbContext.Tickets.Include(t => t.Seat).Take(rowsNumber).ToList();
        }

        public IEnumerable<Ticket> GetFromCache(string cacheKey, int rowsNumber = 20)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Ticket> tickets))
            {
                tickets = GetAllFromDb();
                if (tickets != null)
                {
                    _memoryCache.Set(cacheKey, tickets, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(266)));
                }
            }
            return tickets;
        }
    }
}
