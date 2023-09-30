using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SetsController : ControllerBase
{
    private readonly string SET_CACHE_NAME = System.Configuration.ConfigurationManager.AppSettings["SetsCacheName"] ?? "sets";
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
        if (!_cache.Contains(SET_CACHE_NAME))
        {
            Console.WriteLine("Sets not in cache - Downloading");
            List<MTG_Set> sets = await _scryfall_Connection.GetCollectableSets();
            _cache.Add(SET_CACHE_NAME, sets, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(60 * 24) });
        }

        return (List<MTG_Set>)_cache.Get(SET_CACHE_NAME);
    }
}