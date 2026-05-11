using DDD.AspireShop.Application.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DDD.AspireShop.ApiService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var products = await mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(products);
    }
}
