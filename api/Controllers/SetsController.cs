using System.Reflection.Metadata;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mtg_api;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SetsController : ControllerBase
{
    // private readonly MtgInvContext _context;
    private readonly string SET_CACHE_NAME = System.Configuration.ConfigurationManager.AppSettings["SetsCacheName"] ?? "sets";
    private IScryfall_Connection _scryfall_Connection;
    private MemoryCache _cache;
    private CacheItemPolicy _policy;

    public SetsController(IScryfall_Connection scryfall_Connection, MemoryCache cache, CacheItemPolicy policy)
    {
        _scryfall_Connection = scryfall_Connection;
        _cache = cache;
        _policy = policy;
    }

    // GET: api/Sets
    [HttpGet]
    public async Task<ActionResult<List<MTG_Set>>> GetMTG_Sets()
    {
        if (!_cache.Contains(SET_CACHE_NAME))
        {
            Console.WriteLine("Sets not in cache - Downloading");
            List<MTG_Set> sets = await _scryfall_Connection.GetCollectableSets();
            _cache.Add(SET_CACHE_NAME, sets, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
        }

        return (List<MTG_Set>)_cache.Get(SET_CACHE_NAME);
    }

    // GET: api/Sets/DOM
    // [HttpGet("{id}")]
    // public async Task<ActionResult<MTG_Set>> GetMTG_Set(string id)
    // {
    //     if (_context.Sets == null)
    //     {
    //         return NotFound();
    //     }
    //     var mTG_Set = await _context.Sets.FindAsync(id);

    //     if (mTG_Set == null)
    //     {
    //         return NotFound();
    //     }

    //     return mTG_Set;
    // }

    // private bool MTG_SetExists(string id)
    // {
    //     return (_context.Sets?.ToList().Any(e => e.SetCode == id)).GetValueOrDefault();
    // }
}