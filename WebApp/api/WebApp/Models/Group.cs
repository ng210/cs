namespace WebApp.Models;

public sealed class Group
{
    public required string Name { get; set; }
    public string? Parent { get; set; }
    public List<string> Members { get; set; } = [];
    public List<PermissionRule> Rules { get; set; } = [];
}