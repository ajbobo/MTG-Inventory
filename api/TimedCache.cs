using System.Runtime.Caching;

namespace mtg_api;

public class TimedCache<T>
{
    public delegate void RefreshFunction();

    private MemoryCache cache;
    public int CacheTime { get; set; }

    public event RefreshFunction? OnRefresh;

    private CacheItemPolicy Policy
    {
        get
        {
            return new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddMinutes(CacheTime) };
        }
    }

    public TimedCache(string name, int cacheTime)
    {
        cache = new MemoryCache(name);
        CacheTime = cacheTime;
    }

    public Boolean Contains(string key)
    {
        return cache.Contains(key);
    }

    public T Get(string key)
    {
        if (!cache.Contains(key) && OnRefresh != null)
            OnRefresh();

        return (T)cache.Get(key);
    }

    public void Put(string key, T obj)
    {
        if (obj == null)
            throw new Exception("Null values not allowed in TimedCache");

        cache.Add(key, obj, Policy);
    }

}