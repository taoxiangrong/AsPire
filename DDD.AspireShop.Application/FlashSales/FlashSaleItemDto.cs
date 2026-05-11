namespace DDD.AspireShop.Application.FlashSales;

public sealed record FlashSaleItemDto(
    Guid ProductId,
    string ProductName,
    string Description,
    decimal OriginalPrice,
    decimal FlashPrice,
    string Currency,
    int RemainingStock,
    int LimitPerOrder,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsActive);
