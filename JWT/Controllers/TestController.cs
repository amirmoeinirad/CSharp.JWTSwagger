
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JWTwithSwagger.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]    
    public class TestController : Controller
    {
        // Accessing this action method requires authorization. (Token must be provided by the client GET request.)
        // Therefore, the client must first access the server at /api/auth/login to obtain a JWT token.
        // Then, the client must include this token in the Authorization header of the GET request to access this protected endpoint
        // at this URL: /api/test/secure.
        [HttpGet("secure")]
        [Authorize]
        public IActionResult SecureEndpoint()
        {
            Console.WriteLine("In the protected action method..."); // Printed on the server console.
            return Ok(new { message = "You accessed a protected API endpoint!" });  // Printed on the client console.
        }
    }
}
