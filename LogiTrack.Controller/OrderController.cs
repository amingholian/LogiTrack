using System.Diagnostics;
using LogiTrack.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controller
{
  [ApiController]
  [Route("api/orders")]
  [Produces("application/json")]
  [Authorize]
  public class OrderController : ControllerBase
  {
    private readonly LogiTrackContext _context;

    public OrderController(LogiTrackContext context)
    {
      _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
      var sw = Stopwatch.StartNew();
      var orders = await _context.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .ToListAsync();
      sw.Stop();
      Response.Headers["X-Response-Time-Ms"] = sw.ElapsedMilliseconds.ToString();
      return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetById(int id)
    {
      var order = await _context.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.OrderId == id);

      if (order is null)
        return NotFound(new { message = $"Order with ID {id} was not found." });

      return Ok(order);
    }

    [HttpGet("{id}/items")]
    [ProducesResponseType(typeof(IEnumerable<InventoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetItems(int id)
    {
      // Query items directly — avoids loading the full Order entity
      var items = await _context.InventoryItems
        .AsNoTracking()
        .Where(i => i.OrderId == id)
        .ToListAsync();

      if (!items.Any())
      {
        var exists = await _context.Orders.AnyAsync(o => o.OrderId == id);
        if (!exists)
          return NotFound(new { message = $"Order with ID {id} was not found." });
      }

      return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
      order.DatePlaced = DateTime.UtcNow;

      if (order.Items is { Count: > 0 })
      {
        var ids = order.Items.Select(i => i.ItemId).ToList();
        order.Items = await _context.InventoryItems
          .Where(i => ids.Contains(i.ItemId))
          .ToListAsync();
      }

      _context.Orders.Add(order);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
      var order = await _context.Orders.FindAsync(id);

      if (order is null)
        return NotFound(new { message = $"Order with ID {id} was not found." });

      _context.Orders.Remove(order);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
