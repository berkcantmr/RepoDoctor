namespace RepoDoctor.Cli;

public static class CliParser
{
    public static CliParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
        {
            return CliParseResult.Success(new CliOptions(".", OutputFormat.Text, false));
        }

        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            return CliParseResult.Success(new CliOptions(".", OutputFormat.Text, false, ShowHelp: true));
        }

        if (args.Length == 1 && args[0] is "--version" or "-v")
        {
            return CliParseResult.Success(new CliOptions(".", OutputFormat.Text, false, ShowVersion: true));
        }

        var index = 0;
        if (string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
        {
            index++;
        }

        var path = ".";
        var pathWasSet = false;
        var format = OutputFormat.Text;
        var strict = false;
        string? config = null;
        string? output = null;
        int? minScore = null;
        var listChecks = false;

        while (index < args.Length)
        {
            var current = args[index];
            switch (current)
            {
                case "--help":
                case "-h":
                    return CliParseResult.Success(new CliOptions(".", OutputFormat.Text, false, ShowHelp: true));
                case "--list-checks":
                    listChecks = true;
                    index++;
                    break;
                case "--config":
                case "--output":
                case "--min-score":
                    if (index + 1 >= args.Length || args[index + 1].StartsWith('-'))
                        return CliParseResult.Failure($"{current} requires a value.");
                    var argument = args[index + 1];
                    if (current == "--config") config = argument;
                    else if (current == "--output") output = argument;
                    else
                    {
                        if (!int.TryParse(argument, out var score) || score < 0 || score > 100)
                            return CliParseResult.Failure("--min-score must be an integer between 0 and 100.");
                        minScore = score;
                    }
                    index += 2;
                    break;
                case "--strict":
                    strict = true;
                    index++;
                    break;
                case "--format":
                    if (index + 1 >= args.Length)
                    {
                        return CliParseResult.Failure("--format requires 'text', 'json' or 'markdown'.");
                    }

                    var value = args[index + 1];
                    if (!Enum.GetNames<OutputFormat>().Contains(value, StringComparer.OrdinalIgnoreCase)
                        || !Enum.TryParse<OutputFormat>(value, ignoreCase: true, out format))
                    {
                        return CliParseResult.Failure($"Unsupported format '{value}'. Use 'text', 'json' or 'markdown'.");
                    }

                    index += 2;
                    break;
                default:
                    if (current.StartsWith('-'))
                    {
                        return CliParseResult.Failure($"Unknown option '{current}'.");
                    }

                    if (pathWasSet)
                    {
                        return CliParseResult.Failure("Only one repository path can be scanned at a time.");
                    }

                    path = current;
                    pathWasSet = true;
                    index++;
                    break;
            }
        }

        return CliParseResult.Success(new CliOptions(path, format, strict, Config: config, Output: output, MinScore: minScore, ListChecks: listChecks));
    }
}

public sealed record CliParseResult(CliOptions? Options, string? Error)
{
    public bool IsSuccess => Options is not null;

    public static CliParseResult Success(CliOptions options) => new(options, null);

    public static CliParseResult Failure(string error) => new(null, error);
}
