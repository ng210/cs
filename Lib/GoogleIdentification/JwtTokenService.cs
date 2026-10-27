using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GoogleIdentification
{
    public sealed class JwtTokenService
    {
        private string _issuer;
        private string _audience;
        private string _key;
        private string _clientId;
        public string ClientId => _clientId;
        private string _domain;
        public string Domain => _domain;
        private int _expirationMinutes;

        private SigningCredentials? _credentials = null;

        private static readonly List<string> _claimTypes =
        [
            ClaimTypes.NameIdentifier,
            ClaimTypes.GivenName,
            ClaimTypes.Name,
            ClaimTypes.Email,
            JwtRegisteredClaimNames.Jti,
            "ProfileImageUrl"
        ];

        public JwtTokenService(IConfiguration configuration)
        {
            _clientId = configuration["Client:Id"] ?? throw new InvalidDataException("Missing client id (Client:Id)");
            _domain = configuration["Client:Domain"] ?? throw new InvalidDataException("Missing domain (Client:Domain)");
            _key = configuration["Jwt:Key"] ?? throw new InvalidDataException("Missing JWT key (Jwt:Key)");
            _issuer = configuration["Jwt:Issuer"] ?? throw new InvalidDataException("Missing issuer (Jwt:Issuer)");
            _audience = configuration["Jwt:Audience"] ?? throw new InvalidDataException("Missing audience (Jwt:Audience)");
            _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

            _credentials = new(new SymmetricSecurityKey(Convert.FromBase64String(_key)), SecurityAlgorithms.HmacSha256);
        }

        public static JwtTokenService AddJwtAuthentication(WebApplicationBuilder builder)
        {
            var jwtTokenService = new JwtTokenService(builder.Configuration);
            jwtTokenService.AddAuthentication(builder);
            builder.Services.AddSingleton(jwtTokenService);
            return jwtTokenService;
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
            var validClaims = claims.Where(c => _claimTypes.Contains(c.Type));
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_expirationMinutes),
                signingCredentials: _credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private void AddAuthentication(WebApplicationBuilder builder)
        {
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _issuer,

                        ValidateAudience = true,
                        ValidAudience = _audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_key)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
        }


        // Example: app.MapPost("/api/auth/google", (JwtTokenService tokenService, GoogleLoginRequest request) => tokenService.GoogleLogin(Request.User, request)).AllowAnonymous();
        public async Task<IResult> GoogleLogin(ClaimsPrincipal user, GoogleLoginRequest request)
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
                        Audience = [ClientId]
                    });
            }
            catch (InvalidJwtException)
            {
                return Results.Unauthorized();
            }

            // Szervezeti tagság ellenőrzése.
            if (!string.Equals(payload.HostedDomain, Domain, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            // Csak ellenőrzött e-mail-címet fogadunk el.
            if (string.IsNullOrWhiteSpace(payload.Email) || payload.EmailVerified != true)
            {
                return Results.Unauthorized();
            }

            var accessToken = CreateToken(
            [
                new(ClaimTypes.NameIdentifier, payload.Subject),
                new(ClaimTypes.GivenName, payload.FamilyName),
                new(ClaimTypes.Name, payload.GivenName),
                new(ClaimTypes.Email, payload.Email),
                new("ProfileImageUrl", payload.Picture),

                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]);

            return Results.Text(accessToken, "text/plain");
        }
    }
}