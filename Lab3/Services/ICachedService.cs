namespace Lab3.Services
{
    public interface ICachedService<T>
    {
        public IEnumerable<T> GetAllFromDb(int rowsNumber = 20);
        public void AddIntoCache(string key, int rowsNumber = 20);
        public IEnumerable<T> GetFromCache(string cacheKey, int rowsNumber = 20);
    }
}
