using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MauiBiller.Configuration;

public static class AppConfigurationLoader
{
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

    public static AppConfiguration LoadCurrent()
    {
        var baseNode = LoadJsonNode("appsettings.json");
        var environmentFileName = $"appsettings.{AppEnvironment.Current}.json";
        var environmentNode = LoadJsonNode(environmentFileName);

        MergeInto(baseNode, environmentNode);

        var configuration = baseNode.Deserialize<AppConfiguration>(serializerOptions) ?? new AppConfiguration();

        return new AppConfiguration
        {
            EnvironmentName = string.IsNullOrWhiteSpace(configuration.EnvironmentName)
                ? AppEnvironment.Current
                : configuration.EnvironmentName,
            Firebase = configuration.Firebase,
            Diagnostics = configuration.Diagnostics
        };
    }

    private static JsonObject LoadJsonNode(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded configuration resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return JsonNode.Parse(reader.ReadToEnd())?.AsObject()
            ?? new JsonObject();
    }

    private static void MergeInto(JsonObject target, JsonObject source)
    {
        foreach (var property in source)
        {
            if (property.Value is JsonObject sourceObject)
            {
                if (target[property.Key] is not JsonObject targetObject)
                {
                    targetObject = new JsonObject();
                    target[property.Key] = targetObject;
                }

                MergeInto(targetObject, sourceObject);
                continue;
            }

            target[property.Key] = property.Value?.DeepClone();
        }
    }
}
