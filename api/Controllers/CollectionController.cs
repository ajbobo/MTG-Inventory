using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("api-test")]
namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class CollectionController : ControllerBase
{
    private readonly string CACHE_NAME = "collection";
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
    public async Task<List<CardData>> GetCollection(
        string set,
        [FromQuery(Name = "color")] string colorFilter = "",
        [FromQuery(Name = "count")] string countFilter = "",
        [FromQuery(Name = "rarity")] string rarityFilter = "",
        [FromQuery(Name = "price")] string priceFilter = "",
        [FromQuery(Name = "collectorNumber")] string collectorNumber = "")
    {
        string cacheName = CACHE_NAME + ":" + set;

        // Get all the cards in the set
        var cardList = await GetCardsInSet(set, cacheName);

        // Get all the CTCs for the cards in the collection
        var ctcList = await GetCTCsForSet(set);

        // Combine cards with CTCs
        var joinedList =
            from card in cardList
            join ctc in ctcList on card.CollectorNumber equals ctc.CollectorNumber into temp
            from subcard in temp.DefaultIfEmpty() // LINQ version of left join
            select new CardData()
            {
                Card = card,
                CTCs = subcard?.CTCs ?? null,
                TotalCount = subcard?.TotalCount ?? 0,
            };

        // Filter
        IEnumerable<CardData> rarityList = Filters.FilterByRarity(rarityFilter, joinedList);
        IEnumerable<CardData> countList = Filters.FilterByCount(countFilter, rarityList);
        IEnumerable<CardData> priceList = Filters.FilterByPrice(priceFilter, countList);
        IEnumerable<CardData> colorList = Filters.FilterByColor(colorFilter, priceList);
        IEnumerable<CardData> numberList = Filters.FilterByNumber(collectorNumber, colorList);

        return numberList.ToList();
    }

    internal async Task<List<MTG_Card>> GetCardsInSet(string set, string cacheName)
    {
        if (!_cache.Contains(cacheName))
        {
            Console.WriteLine("Set {0} not in cache - Downloading", cacheName);
            List<MTG_Card> cards = await _scryfall_Connection.GetCardsInSet(set);
            _cache.Add(cacheName, cards, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddMinutes(60 * 24) });
        }

        return (List<MTG_Card>)_cache.Get(cacheName);
    }

    internal async Task<List<CollectionInput>> GetCTCsForSet(string set)
    {
        return await _dbContext.Collection.Where(e => e.SetCode.Equals(set)).ToListAsync();
    }

    // PUT: api/collection/{set}/{card}
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{set}/{card}")]
    public async Task<CollectionInput> PutCollectionEntry(string set, string card, CTCList theList)
    {
        List<MTG_Card> setList = await GetCardsInSet(set, CACHE_NAME);
        MTG_Card? theCard = setList.Find(e => e.CollectorNumber.Equals(card) && e.SetCode.Equals(set));
        string name = theCard?.Name ?? "";
        int total = theList.CTCs.Sum(e => e.Count);

        string key = $"{set}:{card}";
        CollectionInput? curEntry = GetCollectionEntry(key);
        if (curEntry != null)
        {
            curEntry.SetCode = set;
            curEntry.CollectorNumber = card;
            curEntry.Name = name;
            curEntry.Key = key;
            curEntry.CTCs = theList.CTCs;
            curEntry.TotalCount = total;

            _dbContext.Entry(curEntry).State = EntityState.Modified;
        }
        else
        {
            curEntry = new CollectionInput()
            {
                Name = name,
                SetCode = set,
                CollectorNumber = card,
                Key = key,
                CTCs = theList.CTCs,
                TotalCount = total
            };

            _dbContext.Collection.Add(curEntry!);
        }

        await _dbContext.SaveChangesAsync();

        return curEntry;
    }

    // POST: api/collection/{set}/{card}
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("{set}/{card}")]
    public async Task<CollectionInput> PostCollectionEntry(string set, string card, CTCList theList)
    {
        return await PutCollectionEntry(set, card, theList);
    }

    internal CollectionInput? GetCollectionEntry(string? key)
    {
        return _dbContext.Collection?.ToList().Find(e => e.Key.Equals(key));
    }


    // DELETE: api/Collection/{set}/{card}
    [HttpDelete("{set}/{card}")]
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