using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using GoogleAuthenticator.Models;
using GoogleIdentification;


namespace GoogleAuthenticator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Service hozzáadása
            builder.Services.AddAuthorization();
            GoogleJwtTokenService.AddJwtAuthentication(builder);
            builder.Services.AddCors(AddCorsPolicy);

            var app = builder.Build();
            app.UseCors("Frontend");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            #region Auth/Google
            app.MapPost("/api/auth/google",
                (GoogleJwtTokenService tokenService,
                HttpContext context,
                GoogleLoginRequest request) => tokenService.GoogleLogin(context.User, request))
                .AllowAnonymous();

            app.MapDelete("/api/auth/google",
                (GoogleJwtTokenService tokenService,
                HttpContext context) => tokenService.GoogleLogout(context.User))
                .RequireAuthorization();
            #endregion

            #region User
            app.MapGet("/api/users/me",
                (ClaimsPrincipal principal) => Results.Ok(new User
                {
                    Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                    FirstName = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
                    LastName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    ProfileImageUrl = principal.FindFirst("ProfileImageUrl")?.Value ?? ""
                }))
                .RequireAuthorization();
            #endregion

            app.Run();
        }

        private static void AddCorsPolicy(CorsOptions options)
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins([ "http://localhost:5500", "https://localhost:5500" ])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        }
    }
}
