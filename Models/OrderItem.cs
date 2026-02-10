using System.ComponentModel.DataAnnotations;

namespace RestaurantApi.Models;

public class OrderItem
{
    public int Id { get; set; }

    // FK -> Order
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // FK -> MenuItem
    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    
    [Range(0.01, 100000)]
    public decimal UnitPrice { get; set; }

    [Range(0, 1000000)]
    public decimal LineTotal { get; set; }
}
