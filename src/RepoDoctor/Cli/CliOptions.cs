namespace RepoDoctor.Cli;

public sealed record CliOptions(
    string Path,
    OutputFormat Format,
    bool Strict,
    bool ShowHelp = false,
    bool ShowVersion = false);

public enum OutputFormat
{
    Text,
    Json
}
