using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Services;

namespace WebApp
{
    public class Program
    {
        private static string _clientId = "";
        private static string _organizationDomain = "";
        private static string _componentPath = "";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            _clientId = builder.Configuration["Client:Id"] ?? throw new InvalidOperationException("Hiányzó kliens ID.");
            _organizationDomain = builder.Configuration["Web:Domain"] ?? throw new InvalidOperationException("Hiányzó domain.");
            var groupPath = builder.Configuration["Resources:Groups"] ?? throw new InvalidOperationException("Hiányzó groups konfiguráció.");
            _componentPath = builder.Configuration["Resource:Components"] ?? "";

            // Service hozzáadása
            builder.Services.AddAuthorization();
            AddJwtAuthentication(builder);
            builder.Services.AddCors(AddCorsPolicy);
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddSingleton<IPermissionService>(new PermissionService(groupPath));

            var app = builder.Build();
            app.UseCors("Frontend");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapPost("/api/auth/google", GoogleLogin).AllowAnonymous();
            app.MapGet("/api/users/me", GetCurrentUser).RequireAuthorization();

            app.MapGet("/api/components/{id}", GetComponent).RequireAuthorization();

            //app.MapGet("/api/admin/users", GetUsers)
            //   .RequireAuthorization(policy => policy.RequireRole("Admin"))
            //   .WithOpenApi();

            app.Run();
        }

        private static void AddCorsPolicy(CorsOptions options)
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins("http://localhost:5500")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        }

        private static void AddJwtAuthentication(WebApplicationBuilder builder)
        {
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Hiányzó JWT aláírókulcs.");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Hiányzó JWT issuer.");
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Hiányzó JWT audience.");
            var jwtExpirationMinutes = int.Parse(builder.Configuration["Jwt:ExpirationMinutes"] ?? "60");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,

                        ValidateAudience = true,
                        ValidAudience = jwtAudience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
            builder.Services.AddSingleton(new JwtTokenService(jwtKey, jwtIssuer, jwtAudience, jwtExpirationMinutes));
        }

        private static async Task<IResult> GoogleLogin(JwtTokenService tokenService, [FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Credential))
                return Results.BadRequest("Hiányzó Google ID token.");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.Credential,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [_clientId]
                    });
            }
            catch (InvalidJwtException)
            {
                return Results.Unauthorized();
            }

            // Szervezeti tagság ellenőrzése.
            if (!string.Equals(
                    payload.HostedDomain,
                    _organizationDomain,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            // Csak ellenőrzött e-mail-címet fogadunk el.
            if (string.IsNullOrWhiteSpace(payload.Email) || payload.EmailVerified != true)
            {
                return Results.Unauthorized();
            }

            var accessToken = tokenService.CreateToken(
                new User
                {
                    Id = payload.Subject,
                    LastName = payload.FamilyName,
                    FirstName = payload.GivenName,
                    Email = payload.Email,
                    ProfileImageUrl = payload.Picture
                });

            return Results.Text(accessToken, "text/plain");
        }

        private static IResult GetCurrentUser(ClaimsPrincipal user, IUserService userService)
        {
            return Results.Ok(userService.GetCurrentUser(user));
        }

        private static IResult GetComponent(HttpContext context, PermissionService permissionService)
        {
            if (permissionService.HasPermission(context.User, context.Request.Path, Permission.Read))
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, _componentPath, context.Request.Path);
                return Results.File(File.Open(filePath, FileMode.Open));
            } else
            {
                return Results.Forbid();
            }
        }
    }
}
