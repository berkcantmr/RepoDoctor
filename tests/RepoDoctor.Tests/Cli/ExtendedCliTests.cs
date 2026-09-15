using RepoDoctor.Cli;
using RepoDoctor.Scanning;
using RepoDoctor.Reporting;
using RepoDoctor.Tests.Support;

namespace RepoDoctor.Tests.Cli;

public sealed class ExtendedCliTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("99")]
    [InlineData("unknown")]
    public void NumericAndUnknownFormatsAreRejected(string format) =>
        Assert.False(CliParser.Parse(["--format", format]).IsSuccess);

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("abc")]
    public void InvalidThresholdIsRejected(string score) =>
        Assert.False(CliParser.Parse(["--min-score", score]).IsSuccess);

    [Theory]
    [InlineData("--config")]
    [InlineData("--output")]
    [InlineData("--min-score")]
    public void MissingValuesAreRejected(string option) => Assert.False(CliParser.Parse([option]).IsSuccess);

    [Fact]
    public async Task ConfigurationAndThresholdAreApplied()
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile(".repodoctor.json", "{\"minScore\":100,\"disabledChecks\":[\"git\"]}");
        var output = new StringWriter();
        Assert.Equal(1, await RepoDoctorApp.RunAsync([repo.Path, "--format", "json"], output, new StringWriter()));
        Assert.DoesNotContain("\"id\": \"git\"", output.ToString());
        Assert.Equal(0, await RepoDoctorApp.RunAsync([repo.Path, "--min-score", "0"], new StringWriter(), new StringWriter()));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"disabledChecks\":null}")]
    [InlineData("{\"disabledChecks\":[\"typo\"]}")]
    [InlineData("{\"minScore\":101}")]
    [InlineData("{\"excludeDirectories\":[\"../outside\"]}")]
    [InlineData("{\"unknown\":true}")]
    [InlineData("invalid json")]
    public async Task BadConfigurationReturnsUsageError(string configuration)
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile(".repodoctor.json", configuration);
        var error = new StringWriter();
        Assert.Equal(2, await RepoDoctorApp.RunAsync([repo.Path], new StringWriter(), error));
        Assert.Contains("Error:", error.ToString());
    }

    [Fact]
    public async Task OutputNeverOverwritesExistingFile()
    {
        using var repo = new TemporaryRepository();
        var path = Path.Combine(repo.Path, "report.md");
        Assert.Equal(0, await RepoDoctorApp.RunAsync([repo.Path, "--format", "markdown", "--output", path], new StringWriter(), new StringWriter()));
        var original = File.ReadAllText(path);
        Assert.StartsWith("# RepoDoctor report", original);
        Assert.Equal(2, await RepoDoctorApp.RunAsync([repo.Path, "--output", path], new StringWriter(), new StringWriter()));
        Assert.Equal(original, File.ReadAllText(path));
    }

    [Fact]
    public async Task ListChecksAndSubcommandHelpWork()
    {
        var output = new StringWriter();
        Assert.Equal(0, await RepoDoctorApp.RunAsync(["--list-checks"], output, new StringWriter()));
        Assert.Contains("dependency-updates", output.ToString());
        Assert.True(CliParser.Parse(["scan", "--help"]).Options!.ShowHelp);
    }

    [Fact]
    public void MarkdownEscapesUntrustedText()
    {
        var report = new RepositoryReport("<script>|\n", []);
        var result = new MarkdownReportWriter().Write(report);
        Assert.DoesNotContain("<script>", result);
        Assert.Contains("&#124;", result);
    }
}
