using Microsoft.AspNetCore.Mvc;

namespace DDD.AspireShop.ApiService.Controllers;

[ApiController]
[Route("")]
public sealed class HomeController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("DDD AspireShop API is running. Try /api/products and /api/orders.");
    }
}
