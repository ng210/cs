using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Services
{
    public sealed class JwtTokenService
    {
        private string? _issuer = null;
        private string? _audience = null;
        private int _expirationMinutes;

        private SigningCredentials? _credentials = null;

        public JwtTokenService(IConfiguration configuration)
        {
            Initialize(
                configuration["Jwt:Key"]!,
                configuration["Jwt:Issuer"]!,
                configuration["Jwt:Audience"]!,
                int.Parse(configuration["Jwt:ExpirationMinutes"]!));
        }
        public JwtTokenService(string key, string issuer, string audience, int expirationMinutes)
        {
            Initialize(key, issuer, audience, expirationMinutes);
        }

        private void Initialize(string key, string issuer, string audience, int expirationMinutes)
        {
            _issuer = issuer;
            _audience = audience;
            _expirationMinutes = expirationMinutes;
            _credentials = new(new SymmetricSecurityKey(Convert.FromBase64String(key)),SecurityAlgorithms.HmacSha256);
        }

        public string CreateToken(User user)
        {
            var now = DateTime.UtcNow;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Name, user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("ProfileImageUrl", user.ProfileImageUrl)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_expirationMinutes),
                signingCredentials: _credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}