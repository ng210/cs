namespace WebApp.Models;

public enum Permission
{
    Read,
    Create,
    Update,
    Delete
}

public sealed class PermissionRule
{
    public string Path { get; set; } = "";
    public Permission[] Allow { get; set; } = [];
    public Permission[] Deny { get; set; } = [];
}