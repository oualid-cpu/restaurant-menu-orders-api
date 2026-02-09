using Microsoft.EntityFrameworkCore;
using RestaurantApi.Models;

namespace RestaurantApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Only seed if there is no menu data yet
        if (await db.MenuItems.AnyAsync())
            return;

        var items = new List<MenuItem>
        {
            new() { Name = "Margherita Pizza", Category = "Pizza", Description = "Tomato, mozzarella, basil", Price = 9.90m, IsAvailable = true },
            new() { Name = "Pepperoni Pizza", Category = "Pizza", Description = "Pepperoni, mozzarella, tomato sauce", Price = 11.50m, IsAvailable = true },
            new() { Name = "Spaghetti Bolognese", Category = "Pasta", Description = "Rich beef ragù, parmesan", Price = 12.90m, IsAvailable = true },
            new() { Name = "Penne Arrabbiata", Category = "Pasta", Description = "Spicy tomato sauce, garlic", Price = 10.90m, IsAvailable = true },
            new() { Name = "Caesar Salad", Category = "Salad", Description = "Romaine, croutons, parmesan, Caesar dressing", Price = 8.50m, IsAvailable = true },
            new() { Name = "Cheeseburger", Category = "Burger", Description = "Beef patty, cheddar, lettuce, tomato", Price = 10.50m, IsAvailable = true },
            new() { Name = "Chicken Wings", Category = "Starter", Description = "6 pcs, spicy glaze", Price = 7.90m, IsAvailable = true },
            new() { Name = "Tomato Soup", Category = "Starter", Description = "Creamy tomato soup, basil oil", Price = 5.90m, IsAvailable = true },
            new() { Name = "Chocolate Brownie", Category = "Dessert", Description = "Warm brownie, vanilla ice cream", Price = 6.50m, IsAvailable = true },
            new() { Name = "Lemonade", Category = "Drink", Description = "Fresh lemon, lightly sweetened", Price = 3.50m, IsAvailable = true }
        };

        await db.MenuItems.AddRangeAsync(items);
        await db.SaveChangesAsync();
    }
}
