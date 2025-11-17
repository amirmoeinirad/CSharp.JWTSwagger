
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JWTwithSwagger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Working with JWT Tokens in .NET");
            Console.WriteLine("-------------------------------\n");


            // Initializing the Web Application
            // WebApplicationBuilder object
            var builder = WebApplication.CreateBuilder(args);


            // --------------------------------------------------------


            // Load JWT settings            
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");

            // Encoding.GetByts() converts the characters in a string into an array of bytes.
            byte[] key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "unknown");


            //////////////
            // SERVICES //
            //////////////


            // (1)
            // Configure authentication using a JWT Token
            // Authentication is performed by extracting and validating a JWT token from the authorization request header.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key) // Symmetric Security Key is used to sign and validate the token.
                        // A Symmetric Security Key means the same key is used for both signing and validation.
                    };
                });


            // (2)
            // Adding the SymmetricSecurityKey as a singleton service.
            // This means that the same instance of SymmetricSecurityKey will be used throughout the application's lifetime.
            builder.Services.AddSingleton(new SymmetricSecurityKey(key));

            // (3)
            // Adding authorization services to the application.
            builder.Services.AddAuthorization();

            // (4)
            // Adding controller services to the application.
            // In other words, enabling the use of MVC controllers.
            builder.Services.AddControllers();

            // (5)
            // Configuring Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                // (a)
                options.SwaggerDoc("v1.0", new OpenApiInfo { Title = "My Test API", Version = "v1.0" });

                // (b)
                // 'AddSecurityDefinition' is used to define a security scheme that can be used by the API.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization", // The request header name for JWT
                    Type = SecuritySchemeType.ApiKey, // Swagger treats Bearer tokens as API keys.
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header, // Expect this token to be sent in the HTTP header.
                    Description = "Enter 'Bearer' [space] and then your valid JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
                });

                // (c)
                // 'AddSecurityRequirement' is used to specify that the API requires a certain security scheme to be used.
                // Now we’re saying: "This API requires JWT security."
                // This class behaves like a dictionary.
                // OpenApiSecurityRequirement is essentially a Dictionary<OpenApiSecurityScheme, IList<string>>.
                options.AddSecurityRequirement(new OpenApiSecurityRequirement // So inside the curly braces, we are adding key-value pairs.
                {
                    {
                        // key
                        new OpenApiSecurityScheme // Referring to 'Bearer' scheme defined above.
                        {
                            Reference = new OpenApiReference // We’re using a reference to the "Bearer" scheme defined above.
                            {
                                Type = ReferenceType.SecurityScheme, // Indicating that this reference is to a security scheme.
                                Id = "Bearer"                        // The ID of the security scheme we are referencing.
                            }
                        },
                        // value
                        new string[] {} // This empty array means: no specific scopes are required (which would be used in OAuth2).
                    }
                });
            });


            // --------------------------------------------------------

            // WebApplication object
            var app = builder.Build();

            // --------------------------------------------------------


            /////////////////
            // MIDDLEWARES //
            /////////////////


            // (1)
            app.UseAuthentication();
            app.UseAuthorization();

            // (2)
            // Enabling attribute routing for controllers.
            app.MapControllers();

            // (3)
            // In older versions of .NET, app.UseRouting() or app.UseEndpoints() can be used instead.
            // app.MapGet() is the minimal APIs model in .NET 6+.
            // Here, we define a simple GET endpoint at the root URL ("/") that returns a plain text message.
            app.MapGet("/", () => "A simple JWT Token application!");

            // (4)
            // Enforcing HTTPS redirection and HSTS (HTTP Strict Transport Security)
            // HSTS is a web security policy mechanism that helps to protect websites
            // from man-in-the-middle attacks by forcing clients to use HTTPS.
            // In fact, HSTS prevents downgrade attacks and cookie hijacking.
            app.UseHsts();

            // (5)
            // Enabling Swagger in the Dev Environment on: http(s)://localhost:port_number/swagger
            if (app.Environment.IsDevelopment())
            {
                // The Swagger middleware handles all incoming requests.
                app.UseSwagger();
                // or instead of app.UseSwagger(): app.MapSwagger("/openapi/{documentName}/swagger.json"); (server-side mapping)
                // app.MapSwagger() directly defines a route endpoint. More modern in .NET 6+.
                app.UseSwaggerUI(c =>
                {
                    // Add custom style
                    c.InjectStylesheet("/css/swagger.css");

                    // 'swagger.json' file is generated by the Swagger middleware.
                    // It contains the OpenAPI specification for the API.
                    c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "My API V1"); // (client-side mapping)

                    // Set a custom route, e.g., localhost:port_number/swagger
                    c.RoutePrefix = "swagger";
                });                
            }

            // (6)
            // Serving static files (e.g., CSS files for Swagger UI) in the wwwroot folder
            app.UseStaticFiles();

            // --------------------------------------------------------

            // Running the application
            app.Run();
        }
    }
}
