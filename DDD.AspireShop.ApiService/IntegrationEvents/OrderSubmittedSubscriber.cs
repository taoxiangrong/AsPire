using DDD.AspireShop.Application.Orders;
using DotNetCore.CAP;

namespace DDD.AspireShop.ApiService.IntegrationEvents;

public sealed class OrderSubmittedSubscriber(
    ILogger<OrderSubmittedSubscriber> logger,
    InMemoryConsumedOrderMessageStore messages) : ICapSubscribe
{
    [CapSubscribe("shop.orders.submitted")]
    public Task HandleAsync(OrderSubmittedIntegrationEvent message)
    {
        logger.LogInformation(
            "CAP received order submitted event. OrderId: {OrderId}, Customer: {CustomerName}, Total: {TotalAmount}",
            message.OrderId,
            message.CustomerName,
            message.TotalAmount);

        messages.Add(new ConsumedOrderMessage(
            message.OrderId,
            message.CustomerName,
            message.TotalAmount,
            message.SubmittedAt,
            DateTimeOffset.UtcNow));

        return Task.CompletedTask;
    }
}
