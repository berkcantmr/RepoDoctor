using System.Reflection;
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

        var report = new RepositoryScanner().Scan(fullPath);
        IReportWriter writer = options.Format switch
        {
            OutputFormat.Json => new JsonReportWriter(),
            _ => new TextReportWriter()
        };

        output.WriteLine(writer.Write(report));
        return Task.FromResult(options.Strict && report.HasFindings ? FindingsExitCode : SuccessExitCode);
    }

    private static string GetVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";

    public const string HelpText = """
        RepoDoctor audits a repository for common open-source essentials.

        Usage:
          repodoctor [scan] [path] [options]

        Options:
          --format <text|json>  Select the output format. Default: text
          --strict              Exit with code 1 when findings are present
          -h, --help            Show this help text
          -v, --version         Show the version

        Examples:
          repodoctor scan .
          repodoctor ../my-project --format json
          repodoctor scan . --strict
        """;
}
