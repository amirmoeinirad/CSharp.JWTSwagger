
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JWTwithSwagger.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]    
    public class TestController : Controller
    {
        // Accessing this action method requires authorization. (Token must be provided by the client GET request.)        
        [HttpGet("secure")]
        [Authorize]
        public IActionResult SecureEndpoint()
        {
            Console.WriteLine("In the protected action method..."); // Printed on the server console.
            return Ok(new { message = "You accessed a protected API endpoint!" });  // Printed on the client console.
        }
    }
}
