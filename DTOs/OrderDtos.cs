namespace RestaurantApi.DTOs;

// --- Create Order ---
public record CreateOrderItemDto(int MenuItemId, int Quantity);

public record CreateOrderDto(List<CreateOrderItemDto> Items);

public record OrderCreatedDto(
    int Id,
    DateTime CreatedAtUtc,
    decimal TotalAmount
);

// --- Read Orders (Summary) ---
public record OrderSummaryDto(
    int Id,
    DateTime CreatedAtUtc,
    int ItemCount,
    decimal TotalAmount
);

// --- Read Orders (Details) ---
public record OrderItemDetailsDto(
    int MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record OrderDetailsDto(
    int Id,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    List<OrderItemDetailsDto> Items
);
