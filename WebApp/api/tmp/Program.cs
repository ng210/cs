using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationApi
{
    public class Program
    {
        private static string _clientId = string.Empty;
        // konfigurációba kell áthelyezni *****
        private static string _organizationDomain = "hbsz.edu.hu";
        private static Dictionary<string, List<string>> _roleEmailMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Admin"] = new()
                {
                        "nemeth.gabor@hbsz.edu.hu"
                },
                ["Tanár"] = new()
                {
                        "horvath.agnes@hbsz.edu.hu"
                }
            };
        // ***********************************

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            _clientId = builder.Configuration["Client:Id"] ?? throw new InvalidOperationException("Hiányzó kliens ID."); ;

            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Hiányzó JWT aláírókulcs.");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Hiányzó JWT issuer.");
            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Hiányzó JWT audience.");
            var jwtExpirationMinutes = int.Parse(builder.Configuration["Jwt:ExpirationMinutes"]
                ?? "60");

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

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            app.UseCors("Frontend");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapPost("/api/auth/google", GoogleLogin)
                .AllowAnonymous()
                .WithOpenApi();

            app.MapGet("/api/users/me", GetCurrentUser)
               .RequireAuthorization()
               .WithOpenApi();

            //app.MapGet("/api/admin/users", GetUsers)
            //   .RequireAuthorization(policy => policy.RequireRole("Admin"))
            //   .WithOpenApi();

            app.Run();
        }

        private static IResult GetCurrentUser(HttpContext context)
        {
            var claims = context.User;

            var user = new User
            {
                Id = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "n.a.",
                FirstName = claims.FindFirst(ClaimTypes.GivenName)?.Value ?? "n.a.",
                LastName = claims.FindFirst(ClaimTypes.Name)?.Value ?? "n.a.",
                Email = claims.FindFirst(ClaimTypes.Email)?.Value ?? "n.a.",
                Role = claims.FindFirst(ClaimTypes.Role)?.Value ?? "Diák",
                ProfileImageUrl = claims.FindFirst("ProfileImageUrl")?.Value ?? "blank.png"
            };

            return Results.Ok(user);
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

            // Alapértelmezett szerepkör.
            var role = "Diák";

            foreach (var (candidateRole, emails) in _roleEmailMap)
            {
                if (emails.Contains(payload.Email, StringComparer.OrdinalIgnoreCase))
                {
                    role = candidateRole;
                    break;
                }
            }

            var accessToken = tokenService.CreateToken(
                new User {
                    Id = payload.Subject,
                    LastName = payload.FamilyName,
                    FirstName = payload.GivenName,
                    Email = payload.Email,
                    Role = role,
                    ProfileImageUrl = payload.Picture
                });

            return Results.Text(accessToken, "text/plain");
        }
    }
}
