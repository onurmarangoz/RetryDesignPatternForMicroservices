using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            return Ok(new { Id = id , Name = "Bilgisayar", Price = 20000, Stock = 200, Category = "Elektronik Eşya" });
        }
    }
}
