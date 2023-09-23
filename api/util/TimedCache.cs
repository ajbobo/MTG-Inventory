using System.Runtime.Caching;

namespace mtg_api;

public class TimedCache<T> : ITimedCache<T>
{
    public delegate T RefreshFunction(string key);

    private MemoryCache cache;
    public int CacheTime { get; set; } = 10;

    public event RefreshFunction? OnRefresh;

    private CacheItemPolicy Policy
    {
        get
        {
            return new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(CacheTime) };
        }
    }

    public TimedCache()
    {
        cache = MemoryCache.Default;
    }

    public bool Contains(string key)
    {
        return cache.Contains(key);
    }

    public T Get(string key)
    {
        if (!cache.Contains(key) && OnRefresh != null)
        {
            T obj = OnRefresh(key);
            if (obj != null)
                cache.Add(key, obj, Policy);
        }

        return (T)cache.Get(key);
    }

    public void Put(string key, T obj)
    {
        if (obj == null)
            throw new Exception("Null values not allowed in TimedCache");

        cache.Add(key, obj, Policy);
    }

}