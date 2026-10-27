using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Services;

public interface IPermissionService
{
    bool HasPermission(ClaimsPrincipal user, string path, Permission permission);
    Permission[] GetPermissions(User user, string path);
}