
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
            // Authorization is performed by extracting and validating a JWT token from the authorization request header.
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
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });


            // (2)
            // Adding th symmetric security key as a service so that it could be injected into other methods elsewhere.
            builder.Services.AddSingleton(new SymmetricSecurityKey(key));

            // (3)
            builder.Services.AddAuthorization();

            // (4)
            builder.Services.AddControllers();

            // (5)
            // Configuring Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                // (a)
                options.SwaggerDoc("v1.0", new OpenApiInfo { Title = "My Test API", Version = "v1.0" });
                
                // (b)
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
                // Now we’re saying: "This API requires JWT security."
                // This class behaves like a dictionary.                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement // So inside the curly braces, we are adding key-value pairs.
                {
                    {
                        // key
                        new OpenApiSecurityScheme // Referring to 'Bearer' scheme defined above.
                        {
                            Reference = new OpenApiReference // We’re using a reference to the "Bearer" scheme defined above.
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
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
            app.MapControllers();
            
            // (3)
            // In older versions of .NET, app.UseRouting() or app.UseEndpoints() can be used instead.
            // app.MapGet() is the minimal APIs model in .NET 6+.
            app.MapGet("/", () => "A simple JWT Token application!");

            // (4)
            app.UseHsts();

            // (5)
            // Enabling Swagger on: http(s)://localhost:port_number/swagger
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

                    c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "My API V1"); // (client-side mapping)

                    // Set a custom route, e.g., localhost:port_number/swagger
                    c.RoutePrefix = "swagger";
                });                
            }

            // (6)
            app.UseStaticFiles();

            // --------------------------------------------------------

            app.Run();
        }
    }
}
