using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Models;
using Group = WebApp.Models.Group;

namespace WebApp.Services;

public class PermissionService : IPermissionService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };
    private readonly PermissionConfiguration _config;

    private static PermissionConfiguration Load(string path)
    {
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<PermissionConfiguration>(json, Options)
                        ?? throw new InvalidDataException("Empty permission configuration.");

        var groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
        var members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in config.Groups)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                throw new InvalidDataException("Group name cannot be empty.");
            }

            if (!groups.TryAdd(group.Name, group))
            {
                throw new InvalidDataException($"Duplicate group name: {group.Name}");
            }

            foreach (var email in group.Members)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new InvalidDataException($"Invalid email '{email}'.");
                }

                if (!members.Add(email))
                {
                    throw new InvalidDataException($"Email '{email}' belongs to multiple groups.");
                }
            }

            foreach (var rule in group.Rules)
            {
                if (string.IsNullOrWhiteSpace(rule.Path))
                {
                    throw new InvalidDataException($"Empty path in group '{group.Name}'.");
                }

                if (rule.Allow.Intersect(rule.Deny).Any())
                {
                    throw new InvalidDataException($"Permission conflict in '{group.Name}', path '{rule.Path}'.");
                }

            }
        }

        foreach (var group in config.Groups)
        {
            if (group.Parent != null && !groups.ContainsKey(group.Parent))
            {
                throw new InvalidDataException($"Unknown parent '{group.Parent}' in group '{group.Name}'.");
            }
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Visit(Group group)
        {
            if (active.Contains(group.Name))
            {
                throw new InvalidDataException($"Circular group hierarchy at '{group.Name}'.");
            }

            if (!visited.Add(group.Name))
            {
                return;
            }

            active.Add(group.Name);

            if (group.Parent != null)
            {
                Visit(groups[group.Parent]);
            }

            active.Remove(group.Name);
        }

        foreach (var group in config.Groups)
        {
            Visit(group);
        }

        return config;
    }

    private static bool Matches(string pattern, string path)
    {
        var p = pattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var s = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < p.Length; i++)
        {
            if (p[i] == "**")
                return i == p.Length - 1 && s.Length > i;

            if (i >= s.Length)
                return false;

            if (p[i] != "*" &&
                !string.Equals(p[i], s[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return p.Length == s.Length;
    }

    private bool? EvaluateGroup(Group group, string path, Permission permission)
    {
        bool? decision = null;

        foreach (var rule in group.Rules)
        {
            if (!Matches(rule.Path, path))
                continue;

            if (rule.Deny.Contains(permission))
                return false;

            if (rule.Allow.Contains(permission))
                decision = true;
        }

        if (decision.HasValue)
            return decision;

        if (group.Parent is not null)
        {
            var parent = _config!.Groups.First(
                g => g.Name.Equals(group.Parent,
                    StringComparison.OrdinalIgnoreCase));

            return EvaluateGroup(parent, path, permission);
        }

        return false;
    }

    public PermissionService(string path)
    {
        _config = Load(path);
    }

    public bool HasPermission(ClaimsPrincipal user, string path, Permission permission) {
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email)) return false;
        var group = _config.Groups.FirstOrDefault(g => g.Members.Contains(email, StringComparer.OrdinalIgnoreCase));
        if (group == null)
            return false;

        return EvaluateGroup(group, path, permission) == true;
    }

    Permission[] IPermissionService.GetPermissions(User user, string path) {
        throw new NotImplementedException();
    }
}