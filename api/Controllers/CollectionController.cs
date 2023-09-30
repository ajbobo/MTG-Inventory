using System.Reflection.Metadata;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // GET: api/Collection/{set}?[filters]
    [HttpGet("{set}")]
    public async Task<List<CardResult>> GetCollection(
        string set,
        [FromQuery(Name = "color")] string colorFilter = "",
        [FromQuery(Name = "count")] string countFilter = "",
        [FromQuery(Name = "rarity")] string rarityFilter = "",
        [FromQuery(Name = "cost")] string costFilter = "")
    {
        string cacheName = COLLECTION_CACHE_NAME + ":" + set;

        // First - Get all the cards in the set
        var cardList = await GetCardsInSet(set, cacheName);

        // Second - Get all the CTCs for the cards in the collection
        var ctcList = await GetCTCsForSet(set);

        // Combine cards with CTCs
        var joinedList =
            from card in cardList
            join ctc in ctcList on card.CollectorNumber equals ctc.CollectorNumber into temp
            from subcard in temp.DefaultIfEmpty() // LINQ version of left join
            select new CardResult()
            {
                Card = card,
                CTCs = subcard?.CTCs ?? null
            };

        // Third - Filter
        // FINISH ME

        return joinedList.ToList();
    }

    private async Task<List<MTG_Card>> GetCardsInSet(string set, string cacheName)
    {
        if (!_cache.Contains(cacheName))
        {
            Console.WriteLine("Set {0} not in cache - Downloading", cacheName);
            List<MTG_Card> cards = await _scryfall_Connection.GetCardsInSet(set);
            _cache.Add(cacheName, cards, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddMinutes(60 * 24) });
        }

        return (List<MTG_Card>)_cache.Get(cacheName);
    }

    private async Task<List<CollectionEntry>> GetCTCsForSet(string set)
    {
        return await _dbContext.Collection.Where(e => e.SetCode.Equals(set)).ToListAsync();
    }

    // PUT: api/collection/{set}/card/{card}
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{set}/card/{card}")]
    public async Task<IActionResult> PutCollectionEntry(string set, string card, CollectionEntry collectionEntry)
    {
        collectionEntry.SetCode = set;
        collectionEntry.CollectorNumber = card;
        collectionEntry.Key = set + ":" + card;
        // TODO: Get the name from the cache instead of relying on the user entering it
        int total = 0;
        foreach (CardTypeCount ctc in collectionEntry.CTCs)
        {
            total += ctc.Count;
        }
        collectionEntry.TotalCount = total;

        CollectionEntry? curEntry = GetCollectionEntry(collectionEntry.Key);
        if (curEntry != null)
        {
            curEntry.SetCode = collectionEntry.SetCode;
            curEntry.CollectorNumber = collectionEntry.CollectorNumber;
            curEntry.Name = collectionEntry.Name;
            curEntry.Key = collectionEntry.Key;
            curEntry.CTCs = collectionEntry.CTCs;
            curEntry.TotalCount = collectionEntry.TotalCount;
        }

        if (curEntry == null)
        {
            _dbContext.Collection.Add(collectionEntry);
        }
        else
        {
            _dbContext.Entry(curEntry).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync();


        return NoContent();
    }

    // POST: api/collection/{set}/card/{card}
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("{set}/card/{card}")]
    public async Task<IActionResult> PostCollectionEntry(string set, string card, CollectionEntry collectionEntry)
    {
        return await PutCollectionEntry(set, card, collectionEntry);
    }

    private CollectionEntry? GetCollectionEntry(string? key)
    {
        return _dbContext.Collection?.ToList().Find(e => e.Key.Equals(key));
    }


    // DELETE: api/Collection/{set}/card/{card}
    [HttpDelete("{set}/card/{card}")]
    public async Task<IActionResult> DeleteCollectionEntry(string set, string card)
    {
        string key = set + ":" + card;

        if (_dbContext.Collection == null)
            return NotFound();

        var collectionEntry = await _dbContext.Collection.FindAsync(key);
        if (collectionEntry == null)
            return NotFound();

        _dbContext.Collection.Remove(collectionEntry);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}