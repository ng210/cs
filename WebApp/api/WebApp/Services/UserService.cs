using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Services;

public sealed class UserService : IUserService
{
    public User GetCurrentUser(ClaimsPrincipal principal)
    {
        return new User
        {
            Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            FirstName = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
            LastName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
            ProfileImageUrl = principal.FindFirst("ProfileImageUrl")?.Value ?? ""
        };
    }
}
