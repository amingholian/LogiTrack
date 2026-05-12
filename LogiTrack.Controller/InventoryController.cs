using System.Diagnostics;
using LogiTrack.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Controller
{
  [ApiController]
  [Route("api/inventory")]
  [Produces("application/json")]
  [Authorize]
  public class InventoryController : ControllerBase
  {
    private const string CacheKey = "inventory_all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly LogiTrackContext _context;
    private readonly IMemoryCache _cache;

    public InventoryController(LogiTrackContext context, IMemoryCache cache)
    {
      _context = context;
      _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll()
    {
      var sw = Stopwatch.StartNew();

      if (!_cache.TryGetValue(CacheKey, out List<InventoryItem>? items))
      {
        items = await _context.InventoryItems
          .AsNoTracking()
          .ToListAsync();

        _cache.Set(CacheKey, items, CacheDuration);
        Response.Headers["X-Cache"] = "MISS";
      }
      else
      {
        Response.Headers["X-Cache"] = "HIT";
      }

      sw.Stop();
      Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();

      return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(InventoryItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InventoryItem>> Create([FromBody] InventoryItem item)
    {
      _context.InventoryItems.Add(item);
      await _context.SaveChangesAsync();
      _cache.Remove(CacheKey);
      return CreatedAtAction(nameof(GetAll), new { id = item.ItemId }, item);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
      var item = await _context.InventoryItems.FindAsync(id);

      if (item is null)
        return NotFound(new { message = $"InventoryItem with ID {id} was not found." });

      _context.InventoryItems.Remove(item);
      await _context.SaveChangesAsync();
      _cache.Remove(CacheKey);
      return NoContent();
    }
  }
}
