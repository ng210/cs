using System.Security.Claims;
using AdventApi.Models;

namespace AdventApi.Services;

public sealed class PlayerService : IUserService
{
    public Player GetCurrentUser(ClaimsPrincipal principal)
    {
        return new Player
        {
            Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            FirstName = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
            LastName = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "",
            ProfileImageUrl = principal.FindFirst("ProfileImageUrl")?.Value ?? ""
        };
    }
}
