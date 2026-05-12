using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.Model
{
  public class InventoryItem
  {
    [Key]
    public int ItemId { get; set; }
    [Required]
    public required string Name { get; set; }
    public int Quantity { get; set; }
    [Required]
    public required string Location { get; set; }

    [ForeignKey(nameof(Order))]
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public void DisplayInfo()
    {
      Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
    }
  }
}
