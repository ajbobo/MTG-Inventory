using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class CollectionController : ControllerBase
{
    private readonly string COLLECTION_CACHE_NAME = System.Configuration.ConfigurationManager.AppSettings["CollectionCacheName"] ?? "collection";
    private readonly MtgInvContext _dbContext;
    private readonly IScryfall_Connection _scryfall_Connection;
    private MemoryCache _cache;

    public CollectionController(MtgInvContext dbContext, IScryfall_Connection scryfall_Connection, MemoryCache cache)
    {
        _dbContext = dbContext;
        _scryfall_Connection = scryfall_Connection;
        _cache = cache;
    }

    // GET: api/Collection/{set}
    [HttpGet]
    public async Task<ActionResult<List<MTG_Card>>> GetCollection(string set)
    {
        string cacheName = COLLECTION_CACHE_NAME + ":" + set;

        // First - Get all the cards in the set
        if (!_cache.Contains(cacheName))
        {
            Console.WriteLine("Collection {0} not in cache - Downloading", cacheName);
            List<MTG_Card> cards = await _scryfall_Connection.GetCardsInSet(set);
            _cache.Add(cacheName, cards, new CacheItemPolicy()  { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
        }

        return (List<MTG_Card>)_cache.Get(cacheName); // For now

        // Second - Get all the CTCs for the cards in the collection
        // FINIDH ME

        // Third - Filter
        // FINISH ME

        // if (_dbContext.Collection == null)
        // {
        //     return NotFound();
        // }
        // return await _dbContext.Collection.ToListAsync();
    }

    // // PUT: api/Collection/5
    // // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // [HttpPut("{id}")]
    // public async Task<IActionResult> PutCollectionEntry(Guid? id, CollectionEntry collectionEntry)
    // {
    //     if (id != collectionEntry.Uuid)
    //     {
    //         return BadRequest();
    //     }

    //     _dbContext.Entry(collectionEntry).State = EntityState.Modified;

    //     try
    //     {
    //         await _dbContext.SaveChangesAsync();
    //     }
    //     catch (DbUpdateConcurrencyException)
    //     {
    //         if (!CollectionEntryExists(id))
    //         {
    //             return NotFound();
    //         }
    //         else
    //         {
    //             throw;
    //         }
    //     }

    //     return NoContent();
    // }

    // // POST: api/Collection
    // // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // [HttpPost]
    // public async Task<ActionResult<CollectionEntry>> PostCollectionEntry(CollectionEntry collectionEntry)
    // {
    //     if (_dbContext.Collection == null)
    //     {
    //         return Problem("Entity set 'MtgInvContext.Collection'  is null.");
    //     }
    //     _dbContext.Collection.Add(collectionEntry);
    //     await _dbContext.SaveChangesAsync();

    //     return CreatedAtAction("GetCollectionEntry", new { id = collectionEntry.Uuid }, collectionEntry);
    // }

    // // DELETE: api/Collection/5
    // [HttpDelete("{id}")]
    // public async Task<IActionResult> DeleteCollectionEntry(Guid? id)
    // {
    //     if (_dbContext.Collection == null)
    //     {
    //         return NotFound();
    //     }
    //     var collectionEntry = await _dbContext.Collection.FindAsync(id);
    //     if (collectionEntry == null)
    //     {
    //         return NotFound();
    //     }

    //     _dbContext.Collection.Remove(collectionEntry);
    //     await _dbContext.SaveChangesAsync();

    //     return NoContent();
    // }

    // private bool CollectionEntryExists(Guid? id)
    // {
    //     return (_dbContext.Collection?.Any(e => e.Uuid == id)).GetValueOrDefault();
    // }
}