using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("api-test")]
namespace mtg_api;

[Route("api/Decks")]
[ApiController]
public class DeckController : ControllerBase
{
    private readonly MtgInvContext _dbContext;

    public DeckController(MtgInvContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: api/Decks
    [HttpGet()]
    public async Task<List<DeckOutput>> GetDecks()
    {
        return await _dbContext.Decks.Select(p => new DeckOutput{Name = p.Name, Key = p.Key}).ToListAsync();
    }

    // GET: api/Decks/{key}
    [HttpGet("{key}")]
    public async Task<List<DeckData>> GetSingleDeck(string key)
    {
        return await _dbContext.Decks.Where(e => e.Key.Equals(key)).ToListAsync();
    }

    // PUT: api/decks
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut()]
    public async Task<DeckOutput> PutDeckEntry(DeckData theDeck)
    {
        string key = theDeck.Key;
        if (key.Length == 0)
        {
            key = Guid.NewGuid().ToString();
            theDeck.Key = key;
        }

        DeckData? curEntry = GetDeckEntry(key);
        if (curEntry != null)
        {
            curEntry.Name = theDeck.Name;
            curEntry.Key = key;
            curEntry.Cards = theDeck.Cards;

            _dbContext.Entry(curEntry).State = EntityState.Modified;
        }
        else
        {
            curEntry = new DeckData()
            {
                Name = theDeck.Name,
                Key = key,
                Cards = theDeck.Cards,
            };

            _dbContext.Decks.Add(curEntry!);
        }

        await _dbContext.SaveChangesAsync();

        DeckOutput res = new()
        {
            Name = theDeck.Name,
            Key = theDeck.Key
        };

        return res;
    }

    // POST: api/decks
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost()]
    public async Task<DeckOutput> PostDeckEntry(DeckData theDeck)
    {
        return await PutDeckEntry(theDeck);
    }

    internal DeckData? GetDeckEntry(string? key)
    {
        return _dbContext.Decks?.ToList().Find(e => e.Key.Equals(key));
    }


    // DELETE: api/decks/{key}
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteDeckEntry(string key)
    {
        if (_dbContext.Decks == null)
            return NotFound();

        var deckEntry = await _dbContext.Decks.FindAsync(key);
        if (deckEntry == null)
            return NotFound();

        _dbContext.Decks.Remove(deckEntry);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}