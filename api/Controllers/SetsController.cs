using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mtg_api;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SetsController : ControllerBase
{
    // private readonly MtgInvContext _context;
    IScryfall_Connection _scryfall_Connection;
    ITimedCache<MTG_Obj> _cache;

    public SetsController(IScryfall_Connection scryfall_Connection, ITimedCache<MTG_Obj> cache)
    {
        _scryfall_Connection = scryfall_Connection;
        _cache = cache;
    }

    // GET: api/Sets
    [HttpGet]
    public async Task<ActionResult<List<MTG_Set>>> GetMTG_Sets()
    {
        
        return await _scryfall_Connection.GetCollectableSets();
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