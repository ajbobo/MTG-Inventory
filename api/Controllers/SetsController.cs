using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SetsController : ControllerBase
{
    private readonly string CACHE_NAME = "sets";
    private readonly IScryfall_Connection _scryfall_Connection;
    private readonly MemoryCache _cache;

    public SetsController(IScryfall_Connection scryfall_Connection, MemoryCache cache)
    {
        _scryfall_Connection = scryfall_Connection;
        _cache = cache;
    }

    // GET: api/Sets
    [HttpGet]
    public async Task<ActionResult<List<MTG_Set>>> GetMTG_Sets()
    {
        await UpdateCache();

        return (List<MTG_Set>)_cache.Get(CACHE_NAME);
    }

    [HttpGet("{set}")]
    public async Task<ActionResult<List<MTG_Set>>> GetSingleMTG_Set(string set)
    {
        await UpdateCache();

        List<MTG_Set> fullList = (List<MTG_Set>)_cache.Get(CACHE_NAME);
        MTG_Set? found = fullList.Find(x => x.Code.ToLower().Equals(set.ToLower()));

        List<MTG_Set> res = new();
        if (found != null)
            res.Add(found);

        return res;
    }

    private async Task UpdateCache()
    {
        if (!_cache.Contains(CACHE_NAME))
        {
            Console.WriteLine("Sets not in cache - Downloading");
            List<MTG_Set> sets = await _scryfall_Connection.GetCollectableSets();
            _cache.Add(CACHE_NAME, sets, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddMinutes(60 * 24) });
        }
    }
}