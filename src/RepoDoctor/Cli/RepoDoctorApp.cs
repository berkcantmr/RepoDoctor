using System.Reflection;
using System.Text.Json;
using RepoDoctor.Reporting;
using RepoDoctor.Scanning;

namespace RepoDoctor.Cli;

public static class RepoDoctorApp
{
    public const int SuccessExitCode = 0;
    public const int FindingsExitCode = 1;
    public const int UsageErrorExitCode = 2;

    public static Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var parsed = CliParser.Parse(args);
        if (!parsed.IsSuccess)
        {
            error.WriteLine($"Error: {parsed.Error}");
            error.WriteLine("Run 'repodoctor --help' for usage information.");
            return Task.FromResult(UsageErrorExitCode);
        }

        var options = parsed.Options!;
        if (options.ShowHelp)
        {
            output.WriteLine(HelpText);
            return Task.FromResult(SuccessExitCode);
        }

        if (options.ShowVersion)
        {
            output.WriteLine(GetVersion());
            return Task.FromResult(SuccessExitCode);
        }

        if (options.ListChecks)
        {
            output.WriteLine(string.Join(Environment.NewLine, RepositoryScanner.CheckIds));
            return Task.FromResult(SuccessExitCode);
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(options.Path);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            error.WriteLine($"Error: Invalid repository path. {exception.Message}");
            return Task.FromResult(UsageErrorExitCode);
        }

        if (!Directory.Exists(fullPath))
        {
            error.WriteLine($"Error: Directory does not exist: {fullPath}");
            return Task.FromResult(UsageErrorExitCode);
        }

        try
        {
        var configPath = options.Config is null ? Path.Combine(fullPath, ".repodoctor.json") : Path.GetFullPath(options.Config);
        var configuration = options.Config is not null || File.Exists(configPath)
            ? ScanConfiguration.Load(configPath) : new ScanConfiguration();
        var report = new RepositoryScanner().Scan(fullPath, configuration);
        IReportWriter writer = options.Format switch
        {
            OutputFormat.Json => new JsonReportWriter(),
            OutputFormat.Markdown => new MarkdownReportWriter(),
            _ => new TextReportWriter()
        };

        var rendered = writer.Write(report);
        if (options.Output is null) output.WriteLine(rendered);
        else
        {
            // Never overwrite repository files or follow an existing output symlink.
            using var stream = new FileStream(Path.GetFullPath(options.Output), FileMode.CreateNew, FileAccess.Write);
            using var file = new StreamWriter(stream);
            file.WriteLine(rendered);
        }
        var threshold = options.MinScore ?? configuration.MinScore;
        return Task.FromResult((options.Strict && report.HasFindings) || (threshold.HasValue && report.Score < threshold.Value)
            ? FindingsExitCode : SuccessExitCode);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or JsonException or NotSupportedException)
        {
            error.WriteLine($"Error: {exception.Message}");
            return Task.FromResult(UsageErrorExitCode);
        }
    }

    private static string GetVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";

    public const string HelpText = """
        RepoDoctor audits a repository for common open-source essentials.

        Usage:
          repodoctor [scan] [path] [options]

        Options:
          --format <text|json|markdown>  Select output format. Default: text
          --config <path>       Load JSON configuration (default: <repo>/.repodoctor.json)
          --output <path>       Write to a new file; existing files are never overwritten
          --min-score <0-100>   Exit 1 below this score; overrides configuration
          --list-checks        List supported check IDs
          --strict              Exit with code 1 when findings are present
          -h, --help            Show this help text
          -v, --version         Show the version

        Examples:
          repodoctor scan .
          repodoctor ../my-project --format json
          repodoctor scan . --strict
        """;
}
