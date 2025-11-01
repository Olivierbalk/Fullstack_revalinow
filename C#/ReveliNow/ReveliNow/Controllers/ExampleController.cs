using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ReveliNow.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ExampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "OpenAPI werkt!" });
    }
}
