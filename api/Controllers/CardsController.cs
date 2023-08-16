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
public class CardsController : ControllerBase
{
    private readonly MtgInvContext _context;

    public CardsController(MtgInvContext context)
    {
        _context = context;
    }

    // GET: api/Cards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MTG_Card>>> GetCards()
    {
        if (_context.Cards == null)
        {
            return NotFound();
        }
        return await _context.Cards.ToListAsync();
    }

    // GET: api/Cards/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MTG_Card>> GetMTG_Card(Guid? id)
    {
        if (_context.Cards == null)
        {
            return NotFound();
        }
        var mTG_Card = await _context.Cards.FindAsync(id);

        if (mTG_Card == null)
        {
            return NotFound();
        }

        return mTG_Card;
    }

    // PUT: api/Cards/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMTG_Card(Guid? id, MTG_Card mTG_Card)
    {
        if (id != mTG_Card.Uuid)
        {
            return BadRequest();
        }

        _context.Entry(mTG_Card).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MTG_CardExists(id))
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

    // POST: api/Cards
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MTG_Card>> PostMTG_Card(MTG_Card mTG_Card)
    {
        if (_context.Cards == null)
        {
            return Problem("Entity set 'MtgInvContext.Cards'  is null.");
        }
        if (MTG_CardExists(mTG_Card.Uuid))
        {
            return Conflict();
        }

        _context.Cards.Add(mTG_Card);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMTG_Card", new { id = mTG_Card.Uuid }, mTG_Card);
    }

    // DELETE: api/Cards/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMTG_Card(Guid? id)
    {
        if (_context.Cards == null)
        {
            return NotFound();
        }
        var mTG_Card = await _context.Cards.FindAsync(id);
        if (mTG_Card == null)
        {
            return NotFound();
        }

        _context.Cards.Remove(mTG_Card);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MTG_CardExists(Guid? id)
    {
        return (_context.Cards?.ToList().Any(e => e.Uuid == id)).GetValueOrDefault();
    }
}