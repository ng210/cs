using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApp.Models;

namespace WebApp.Services;

public sealed class ResourceRegistry
{
    public Dictionary<string, ResourceDefinition> Resources { get; private set; } = [];

    public static ResourceRegistry Load(string filePath)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());

        using var document = JsonDocument.Parse(File.ReadAllText(filePath));

        var modelTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(typeof(ModelAttribute), false))
            .ToArray();

        var duplicates = modelTypes
            .GroupBy(t => t.FullName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InvalidDataException($"Duplicate model types: {string.Join(", ", duplicates)}");
        }

        var modelTypeMap = modelTypes.ToDictionary(t => t.FullName!);

        var resources = new Dictionary<string, ResourceDefinition>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var name = property.Name;
            var element = property.Value;

            var modelName = element.GetProperty("modelType").GetString()!;

            if (!modelTypeMap.TryGetValue(modelName, out var modelType))
            {
                throw new InvalidDataException($"Unknown model type: {modelName}");
            }

            if (!modelType.IsClass || modelType.IsAbstract || modelType.ContainsGenericParameters)
            {
                throw new InvalidDataException($"Invalid model type: {modelName}");
            }

            var operations = element.GetProperty("operations")
            .Deserialize<Permission[]>(options)
            ?? throw new InvalidDataException($"Missing operations: {name}");

            if (operations.Distinct().Count() != operations.Length ||
                operations.Any(p => !Enum.IsDefined(p)))
                throw new InvalidDataException($"Invalid operations: {name}");

            resources.Add(name, new ResourceDefinition
            {
                Name = name,
                ModelType = modelType,
                Operations = operations
            });
        }
        return new ResourceRegistry { Resources = resources };
    }
}