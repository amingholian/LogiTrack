using LogiTrack.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class LogiTrackContext : IdentityDbContext<ApplicationUser>
{
  public DbSet<InventoryItem> InventoryItems { get; set; }
  public DbSet<Order> Orders { get; set; }
  protected override void OnConfiguring(DbContextOptionsBuilder options)
  => options.UseSqlite("Data Source=logitrack.db");

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Order>()
      .HasMany(o => o.Items)
      .WithOne(i => i.Order)
      .HasForeignKey(i => i.OrderId)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.SetNull);
  }
}