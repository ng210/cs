using System.Security.Claims;
using AdventApi.Models;

namespace AdventApi.Services;

public interface IUserService
{
    Player GetCurrentUser(ClaimsPrincipal principal);
}
