using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? Description { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation
    public List<OrderItem> OrderItems { get; set; } = new();
}
