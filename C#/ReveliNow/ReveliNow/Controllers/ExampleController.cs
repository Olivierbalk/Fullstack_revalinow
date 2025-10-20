using Microsoft.AspNetCore.Mvc;

namespace ReveliNow.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "OpenAPI werkt!" });
    }
}

