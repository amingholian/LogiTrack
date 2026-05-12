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
    public string? CreatedByUserId { get; set; }
    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
  }
}
