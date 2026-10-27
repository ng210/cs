using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Services;

public interface IUserService
{
    User GetCurrentUser(ClaimsPrincipal principal);
}
