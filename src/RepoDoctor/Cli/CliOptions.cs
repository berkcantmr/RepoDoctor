namespace RepoDoctor.Cli;

public sealed record CliOptions(
    string Path,
    OutputFormat Format,
    bool Strict,
    bool ShowHelp = false,
    bool ShowVersion = false,
    string? Config = null,
    string? Output = null,
    int? MinScore = null,
    bool ListChecks = false);

public enum OutputFormat
{
    Text,
    Json,
    Markdown
}
