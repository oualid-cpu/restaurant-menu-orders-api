using Microsoft.EntityFrameworkCore;
using RestaurantApi.Models;

namespace RestaurantApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MenuItem
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Name).IsRequired();
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.LineTotal).HasColumnType("decimal(10,2)");

            // Order (1) -> (many) OrderItem
            entity.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // MenuItem (1) -> (many) OrderItem
            entity.HasOne(x => x.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
