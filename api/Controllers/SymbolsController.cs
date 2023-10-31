using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SymbolsController : ControllerBase
{
    private readonly string CACHE_NAME = "symbols";
    private readonly IScryfall_Connection _scryfall_Connection;
    private readonly MemoryCache _cache;

    public SymbolsController(IScryfall_Connection scryfall_Connection, MemoryCache cache)
    {
        _scryfall_Connection = scryfall_Connection;
        _cache = cache;
    }

    // GET: api/Symbols
    [HttpGet]
    public async Task<ActionResult<List<MTG_Symbol>>> GetMTG_Symbols()
    {
        if (!_cache.Contains(CACHE_NAME))
        {
            Console.WriteLine("Symbols not in cache - Downloading");
            List<MTG_Symbol> sets = await _scryfall_Connection.GetSymbols();
            _cache.Add(CACHE_NAME, sets, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(60 * 24) });
        }

        return (List<MTG_Symbol>)_cache.Get(CACHE_NAME);
    }
}