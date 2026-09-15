using System.Text.Json;
using System.Text.Json.Serialization;

namespace RepoDoctor.Scanning;

public sealed class ScanConfiguration
{
    public string[] DisabledChecks { get; init; } = [];
    public string[] ExcludeDirectories { get; init; } = [];
    public int? MinScore { get; init; }

    public static ScanConfiguration Load(string path)
    {
        var configuration = JsonSerializer.Deserialize<ScanConfiguration>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }) ?? throw new ArgumentException("Configuration must be a JSON object.");
        configuration.Validate();
        return configuration;
    }

    public void Validate()
    {
        if (DisabledChecks is null || ExcludeDirectories is null)
            throw new ArgumentException("Configuration arrays cannot be null.");
        if (MinScore is < 0 or > 100)
            throw new ArgumentException("minScore must be between 0 and 100.");
        foreach (var id in DisabledChecks)
            if (!RepositoryScanner.CheckIds.Contains(id, StringComparer.Ordinal))
                throw new ArgumentException($"Unknown check ID: {id}");
        foreach (var name in ExcludeDirectories)
            if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(['/', '\\', '*', '?']) >= 0)
                throw new ArgumentException("excludeDirectories accepts directory names, not paths or globs.");
    }
}
