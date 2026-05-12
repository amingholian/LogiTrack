using LogiTrack.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controller
{
  [ApiController]
  [Route("api/inventory")]
  [Produces("application/json")]
  [Authorize]
  public class InventoryController : ControllerBase
  {
    private readonly LogiTrackContext _context;

    public InventoryController(LogiTrackContext context)
    {
      _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll()
    {
      var items = await _context.InventoryItems.ToListAsync();
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
      return NoContent();
    }
  }
}
