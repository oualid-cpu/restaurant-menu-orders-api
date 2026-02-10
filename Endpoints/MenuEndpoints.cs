using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantApi.Data;
using RestaurantApi.DTOs;
using RestaurantApi.Models;
using RestaurantApi.Validation;
using Microsoft.AspNetCore.OpenApi;

namespace RestaurantApi.Endpoints;

public static class MenuEndpoints
{
    public static RouteGroupBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/menu")
.WithTags("Menu").WithOpenApi();

        // GET /menu?q=...&category=...&minPrice=...&maxPrice=...
        group.MapGet("/", async (
            AppDbContext db,
            string? q,
            string? category,
            decimal? minPrice,
            decimal? maxPrice) =>
        {
            IQueryable<MenuItem> query = db.MenuItems.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qTrim = q.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(qTrim));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var catTrim = category.Trim().ToLower();
                query = query.Where(m => m.Category.ToLower() == catTrim);
            }

            if (minPrice.HasValue)
                query = query.Where(m => m.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(m => m.Price <= maxPrice.Value);

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["price"] = new[] { "minPrice must be less than or equal to maxPrice." }
                });
            }

            var items = await query
                .OrderBy(m => m.Id)
                .Select(m => new MenuItemDto(
                    m.Id,
                    m.Name,
                    m.Category,
                    m.Description,
                    m.Price,
                    m.IsAvailable
                ))
                .ToListAsync();

            return Results.Ok(items);
        })
        .WithName("GetMenu")
        .Produces<List<MenuItemDto>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // GET /menu/{id}
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return Results.NotFound();

            return Results.Ok(new MenuItemDto(
                item.Id,
                item.Name,
                item.Category,
                item.Description,
                item.Price,
                item.IsAvailable
            ));
        })
        .WithName("GetMenuItemById")
        .Produces<MenuItemDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /menu
        group.MapPost("/", async (
            CreateMenuItemDto dto,
            AppDbContext db,
            IValidator<CreateMenuItemDto> validator) =>
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToProblemDetails());

            var item = new MenuItem
            {
                Name = dto.Name.Trim(),
                Category = dto.Category.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Price = dto.Price,
                IsAvailable = dto.IsAvailable
            };

            db.MenuItems.Add(item);
            await db.SaveChangesAsync();

            var resultDto = new MenuItemDto(
                item.Id,
                item.Name,
                item.Category,
                item.Description,
                item.Price,
                item.IsAvailable
            );

            return Results.Created($"/menu/{item.Id}", resultDto);
        })
        .WithName("CreateMenuItem")
        .Produces<MenuItemDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // PUT /menu/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateMenuItemDto dto,
            AppDbContext db,
            IValidator<UpdateMenuItemDto> validator) =>
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToProblemDetails());

            var item = await db.MenuItems.FindAsync(id);
            if (item == null)
                return Results.NotFound();

            item.Name = dto.Name.Trim();
            item.Category = dto.Category.Trim();
            item.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            item.Price = dto.Price;
            item.IsAvailable = dto.IsAvailable;

            await db.SaveChangesAsync();

            return Results.NoContent();
            // var resultDto = new MenuItemDto(
            //     item.Id,
            //     item.Name,
            //     item.Category,
            //     item.Description,
            //     item.Price,
            //     item.IsAvailable
            // );

            // return Results.Ok(resultDto);

        })
        .WithName("UpdateMenuItem")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // DELETE /menu/{id}
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item == null)
                return Results.NotFound();

            db.MenuItems.Remove(item);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteMenuItem")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
