using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class CollectionController : ControllerBase
{
    private readonly MtgInvContext _context;

    public CollectionController(MtgInvContext context)
    {
        _context = context;
    }

    // GET: api/Collection
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollectionEntry>>> GetCollection()
    {
        if (_context.Collection == null)
        {
            return NotFound();
        }
        return await _context.Collection.ToListAsync();
    }

    // GET: api/Collection/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CollectionEntry>> GetCollectionEntry(Guid? id)
    {
        if (_context.Collection == null)
        {
            return NotFound();
        }
        var collectionEntry = await _context.Collection.FindAsync(id);

        if (collectionEntry == null)
        {
            return NotFound();
        }

        return collectionEntry;
    }

    // PUT: api/Collection/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCollectionEntry(Guid? id, CollectionEntry collectionEntry)
    {
        if (id != collectionEntry.Uuid)
        {
            return BadRequest();
        }

        _context.Entry(collectionEntry).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CollectionEntryExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Collection
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CollectionEntry>> PostCollectionEntry(CollectionEntry collectionEntry)
    {
        if (_context.Collection == null)
        {
            return Problem("Entity set 'MtgInvContext.Collection'  is null.");
        }
        _context.Collection.Add(collectionEntry);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCollectionEntry", new { id = collectionEntry.Uuid }, collectionEntry);
    }

    // DELETE: api/Collection/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollectionEntry(Guid? id)
    {
        if (_context.Collection == null)
        {
            return NotFound();
        }
        var collectionEntry = await _context.Collection.FindAsync(id);
        if (collectionEntry == null)
        {
            return NotFound();
        }

        _context.Collection.Remove(collectionEntry);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CollectionEntryExists(Guid? id)
    {
        return (_context.Collection?.Any(e => e.Uuid == id)).GetValueOrDefault();
    }
}