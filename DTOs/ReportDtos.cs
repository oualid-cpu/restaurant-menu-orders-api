namespace RestaurantApi.DTOs;

public record TopItemDto(
    int MenuItemId,
    string Name,
    int Quantity
);

public record DailyReportDto(
    DateOnly Date,
    int OrderCount,
    decimal TotalRevenue,
    List<TopItemDto> TopItems
);
