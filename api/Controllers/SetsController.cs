using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mtg_api;

namespace mtg_api;

[Route("api/[controller]")]
[ApiController]
public class SetsController : ControllerBase
{
    private readonly MtgInvContext _context;

    public SetsController(MtgInvContext context)
    {
        _context = context;
    }

    // GET: api/Sets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MTG_Set>>> GetMTG_Set()
    {
        if (_context.Sets == null)
        {
            return NotFound();
        }
        return await _context.Sets.ToListAsync();
    }

    // GET: api/Sets/DOM
    [HttpGet("{id}")]
    public async Task<ActionResult<MTG_Set>> GetMTG_Set(string id)
    {
        if (_context.Sets == null)
        {
            return NotFound();
        }
        var mTG_Set = await _context.Sets.FindAsync(id);

        if (mTG_Set == null)
        {
            return NotFound();
        }

        return mTG_Set;
    }

    // PUT: api/Sets/DOM
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMTG_Set(string id, MTG_Set mTG_Set)
    {
        if (id != mTG_Set.SetCode)
        {
            return BadRequest();
        }

        _context.Entry(mTG_Set).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MTG_SetExists(id))
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

    // POST: api/Sets
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MTG_Set>> PostMTG_Set(MTG_Set mTG_Set)
    {
        if (_context.Sets == null)
        {
            return Problem("Entity set 'MtgInvContext.MTG_Set'  is null.");
        }
        if (MTG_SetExists(mTG_Set.SetCode))
        {
            return Conflict();
        }

        _context.Sets.Add(mTG_Set);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetMTG_Set", new { id = mTG_Set.SetCode }, mTG_Set);
    }

    // DELETE: api/Sets/DOM
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMTG_Set(string id)
    {
        if (_context.Sets == null)
        {
            return NotFound();
        }
        var mTG_Set = await _context.Sets.FindAsync(id);
        if (mTG_Set == null)
        {
            return NotFound();
        }

        _context.Sets.Remove(mTG_Set);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MTG_SetExists(string id)
    {
        return (_context.Sets?.ToList().Any(e => e.SetCode == id)).GetValueOrDefault();
    }
}