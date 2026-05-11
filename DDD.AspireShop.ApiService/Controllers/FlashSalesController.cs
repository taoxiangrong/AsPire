using DDD.AspireShop.Application.FlashSales;
using DDD.AspireShop.Application.Orders;
using DDD.AspireShop.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.AspireShop.ApiService.Controllers;

[ApiController]
[Route("api/flash-sales")]
public sealed class FlashSalesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FlashSaleItemDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var items = await mediator.Send(new GetFlashSaleItemsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> SubmitAsync(SubmitFlashSaleOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(SubmitFlashSaleOrderCommand.FromRequest(request), cancellationToken);
            return Created($"/api/orders/{order.Id}", order);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}
