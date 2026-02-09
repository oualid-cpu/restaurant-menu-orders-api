using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;

namespace RestaurantApi.Endpoints;

public static class ReportsEndpoints
{
    public static RouteGroupBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reports")
            .WithTags("Reports")
            .WithOpenApi();

        // GET /reports/daily?date=YYYY-MM-DD
        group.MapGet("/daily", async (string? date, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["date"] = new[] { "date is required. Use format YYYY-MM-DD." }
                });
            }

            if (!DateOnly.TryParse(date, out var day))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["date"] = new[] { "Invalid date. Use format YYYY-MM-DD." }
                });
            }

            var startUtc = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtc = startUtc.AddDays(1);

            var ordersQuery = db.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAtUtc >= startUtc && o.CreatedAtUtc < endUtc);

            var orderCount = await ordersQuery.CountAsync();
            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var topItemsRaw = await db.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order != null && oi.Order.CreatedAtUtc >= startUtc && oi.Order.CreatedAtUtc < endUtc)
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new
                {
                    MenuItemId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(3)
                .ToListAsync();

            var topIds = topItemsRaw.Select(x => x.MenuItemId).ToList();

            var menuNames = await db.MenuItems
                .AsNoTracking()
                .Where(m => topIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            var topDtos = topItemsRaw
                .Select(x =>
                {
                    var name = menuNames.FirstOrDefault(n => n.Id == x.MenuItemId)?.Name ?? "(unknown)";
                    return new TopItemDto(x.MenuItemId, name, x.Quantity);
                })
                .ToList();

            var report = new DailyReportDto(day, orderCount, totalRevenue, topDtos);
            return Results.Ok(report);
        })
        .WithName("GetDailyReport")
        .Produces<DailyReportDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
