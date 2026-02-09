namespace RestaurantApi.DTOs;

public record MenuItemDto(
    int Id,
    string Name,
    string Category,
    string? Description,
    decimal Price,
    bool IsAvailable
);

public record CreateMenuItemDto(
    string Name,
    string Category,
    string? Description,
    decimal Price,
    bool IsAvailable
);

public record UpdateMenuItemDto(
    string Name,
    string Category,
    string? Description,
    decimal Price,
    bool IsAvailable
);
