using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class CacheController : ControllerBase
{
    private readonly MemoryCache _cache;

    public CacheController(MemoryCache cache)
    {
        _cache = cache;
    }

    // GET: api/Cache
    [HttpGet]
    public List<CacheDescription> GetCacheDescriptions()
    {
        var theList = new List<CacheDescription>();

        var iter = (IEnumerable<KeyValuePair<string, object>>)_cache;
        foreach (KeyValuePair<string, object> item in iter)
        {
            var name = item.Key;
            theList.Add(new CacheDescription()
            {
                Name = name,
            });
        }
        
        return theList;
    }

    // DELETE: api/Cache
    [HttpDelete]
    public void DeleteCaches()
    {
        var iter = (IEnumerable<KeyValuePair<string, object>>)_cache;
        foreach (KeyValuePair<string, object> item in iter)
        {
            _cache.Remove(item.Key);
        }
    }
}