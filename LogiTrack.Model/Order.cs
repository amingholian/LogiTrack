using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Model
{
  public class Order
  {
    [Key]
    public int OrderId { get; set; }
    [Required]
    public required string CustomerName { get; set; }
    public DateTime DatePlaced { get; set; }
    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

    public void AddItem(InventoryItem item)
    {
      if (!Items.Any(i => i.ItemId == item.ItemId))
        Items.Add(item);
    }

    public void RemoveItem(int itemId)
    {
      Items.RemoveAll(i => i.ItemId == itemId);
    }

    public string GetOrderSummary()
    {
      return $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {DatePlaced:M/d/yyyy}";
    }

    public static void PrintSummaries(IEnumerable<Order> orders)
    {
      foreach (var order in orders)
        Console.WriteLine(order.GetOrderSummary());
    }
  }
}
