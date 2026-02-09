using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Range(0, 1000000)]
    public decimal TotalAmount { get; set; }

    // Navigation
    public List<OrderItem> Items { get; set; } = new();
}
