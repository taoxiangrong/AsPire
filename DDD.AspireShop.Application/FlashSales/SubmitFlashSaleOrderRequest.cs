namespace DDD.AspireShop.Application.FlashSales;

public sealed record SubmitFlashSaleOrderRequest(string CustomerName, Guid ProductId, int Quantity);
