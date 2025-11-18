
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JWTwithSwagger.Models;

namespace JWTwithSwagger.Controllers
{
    // The controller class that handles authentication-related actions.    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // (1)                
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;


        // (2)        
        public AuthController(IConfiguration config, SymmetricSecurityKey key)
        {
            _config = config;            
            _key = key;
        }


        // (3)
        // Login endpoint
        // [HttpPost("Login")]: This means the controller will respond to POST requests at this URL: /api/auth/login.        
        [HttpPost("login")]
        // 'FromBody' indicates that the data for the 'model' parameter will come from the body of the HTTP request.    
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Dummy check! Normally, you'd authenticate against a database.
            if (model.Username == "admin" && model.Password == "password")
            {
                // A custom method to generate a JWT for the authenticated user.
                var token = GenerateJwtToken(model.Username);

                // Return the token to the client.
                return Ok(new { token });
            }

            // If the credentials are wrong, return a 401 Unauthorized response.
            return Unauthorized();
        }


        // (4)
        // This method is used to create a JWT for the user based on their username.
        private string GenerateJwtToken(string username)
        {                        
            var claims = new[]
            {                
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _config["JwtSettings:Issuer"] ?? ""),
                new Claim(JwtRegisteredClaimNames.Aud, _config["JwtSettings:Audience"] ?? "")
            };

            // These are the credentials used to sign the JWT. The key and the hashing algorithm are used for signing.
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds                
            );


            // Log the claims to debug
            Console.WriteLine("Token Claims:");
            foreach (var claim in token.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }


            // This converts the JwtSecurityToken into a string (the actual token that can be sent to the client).
            // WriteToken() serializes a JWT as a JWE or JWS.
            // 'Serialize' means converting an object into a format that can be easily stored or transmitted.
            // Here, it converts the JWT object into a compact string format.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
