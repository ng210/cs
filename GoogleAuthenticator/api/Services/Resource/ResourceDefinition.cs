namespace WebApp.Services;
public sealed class ResourceDefinition
{
    public string Name { get; set; } = "";
    public Type ModelType { get; set; } = null!;
    public Permission[] Operations { get; set; } = [];

    public string? Path { get; set; };

    // Origin
}