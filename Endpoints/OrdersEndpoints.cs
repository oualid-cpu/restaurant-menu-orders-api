using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;
using RestaurantApi.Validation;

namespace RestaurantApi.Endpoints;

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // POST /orders
        group.MapPost("/", async (
            CreateOrderDto dto,
            AppDbContext db,
            IValidator<CreateOrderDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToProblemDetails());

            var menuItemIds = dto.Items
                .Select(i => i.MenuItemId)
                .Distinct()
                .ToList();

            var menuItems = await db.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .ToListAsync();

            var missingIds = menuItemIds
                .Except(menuItems.Select(m => m.Id))
                .ToList();

            if (missingIds.Count > 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["items"] = new[] { $"Menu item(s) not found: {string.Join(", ", missingIds)}" }
                });
            }

            var order = new Order
            {
                CreatedAtUtc = DateTime.UtcNow,
                TotalAmount = 0m,
                Items = new List<OrderItem>()
            };

            decimal total = 0m;

            foreach (var line in dto.Items)
            {
                var menuItem = menuItems.First(m => m.Id == line.MenuItemId);
                var unitPrice = menuItem.Price;
                var lineTotal = unitPrice * line.Quantity;

                total += lineTotal;

                order.Items.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });
            }

            order.TotalAmount = total;

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var result = new OrderCreatedDto(order.Id, order.CreatedAtUtc, order.TotalAmount);
            return Results.Created($"/orders/{order.Id}", result);
        })
        .WithName("CreateOrder")
        .Produces<OrderCreatedDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /orders (summary)
        group.MapGet("/", async (AppDbContext db) =>
        {
            var orders = await db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.Id)
                .Select(o => new OrderSummaryDto(
                    o.Id,
                    o.CreatedAtUtc,
                    o.Items.Sum(i => i.Quantity),
                    o.TotalAmount
                ))
                .ToListAsync();

            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .Produces<List<OrderSummaryDto>>(StatusCodes.Status200OK);

        // GET /orders/{id} (details)
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            // Explicit join via projection (reliable in EF Core)
            var order = await db.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailsDto(
                    o.Id,
                    o.CreatedAtUtc,
                    o.TotalAmount,
                    o.Items
                        .OrderBy(i => i.Id)
                        .Select(i => new OrderItemDetailsDto(
                            i.MenuItemId,
                            i.MenuItem!.Name,
                            i.Quantity,
                            i.UnitPrice,
                            i.LineTotal
                        ))
                        .ToList()
                ))
                .FirstOrDefaultAsync();

            if (order == null)
                return Results.NotFound();

            return Results.Ok(order);
        })
        .WithName("GetOrderById")
        .Produces<OrderDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
