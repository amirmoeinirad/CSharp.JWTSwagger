
namespace JWTwithSwagger.Models
{
    // This is a simple data model for the login request.
    // It contains the Username and Password that are sent in the POST request body for authentication.
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
