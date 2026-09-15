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

        while (index < args.Length)
        {
            var current = args[index];
            switch (current)
            {
                case "--strict":
                    strict = true;
                    index++;
                    break;
                case "--format":
                    if (index + 1 >= args.Length)
                    {
                        return CliParseResult.Failure("--format requires either 'text' or 'json'.");
                    }

                    var value = args[index + 1];
                    if (!Enum.TryParse<OutputFormat>(value, ignoreCase: true, out format))
                    {
                        return CliParseResult.Failure($"Unsupported format '{value}'. Use 'text' or 'json'.");
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

        return CliParseResult.Success(new CliOptions(path, format, strict));
    }
}

public sealed record CliParseResult(CliOptions? Options, string? Error)
{
    public bool IsSuccess => Options is not null;

    public static CliParseResult Success(CliOptions options) => new(options, null);

    public static CliParseResult Failure(string error) => new(null, error);
}
