namespace DDD.AspireShop.Application.Catalog;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int Stock,
    bool IsActive);
