
using AdventApi.Services;
using GoogleIdentification;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Security.Claims;

namespace AdventApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Service hozzáadása
            builder.Services.AddAuthorization();
            JwtTokenService.AddJwtAuthentication(builder);
            builder.Services.AddScoped<IUserService, PlayerService>();
            builder.Services.AddCors(AddCorsPolicy);

            var app = builder.Build();
            app.UseCors("Frontend");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapPost(
                "/api/auth/google",
                (JwtTokenService tokenService,
                HttpContext context,
                GoogleLoginRequest request) => tokenService.GoogleLogin(context.User, request))
                .AllowAnonymous();

            app.MapGet(
                "/api/users/me",
                (ClaimsPrincipal user,
                IUserService userService) => Results.Ok(userService.GetCurrentUser(user)))
                .RequireAuthorization();

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
    }
}
