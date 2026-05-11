using DDD.AspireShop.ApiService.IntegrationEvents;
using DDD.AspireShop.Application.Orders;
using DDD.AspireShop.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.AspireShop.ApiService.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    IMediator mediator,
    InMemoryConsumedOrderMessageStore consumedMessages) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var orders = await mediator.Send(new GetOrdersQuery(), cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(SubmitOrderCommand.FromRequest(request), cancellationToken);
            return Created($"/api/orders/{order.Id}", order);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("messages")]
    public ActionResult<IReadOnlyCollection<ConsumedOrderMessage>> GetConsumedMessages()
    {
        return Ok(consumedMessages.List());
    }
}
