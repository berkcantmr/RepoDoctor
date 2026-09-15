using System.Text.Json;
using RepoDoctor.Reporting;
using RepoDoctor.Scanning;

namespace RepoDoctor.Tests.Reporting;

public sealed class ReportWriterTests
{
    private static readonly RepositoryReport SampleReport = new(
        "/sample",
        [
            new RepositoryCheck("readme", "README", CheckStatus.Passed, "Found README.md.", string.Empty),
            new RepositoryCheck("license", "License", CheckStatus.Warning, "Missing license.", "Add a license.")
        ]);

    [Fact]
    public void TextWriter_IncludesStatusAndScore()
    {
        var output = new TextReportWriter().Write(SampleReport);

        Assert.Contains("[PASS] README", output);
        Assert.Contains("[WARN] License", output);
        Assert.Contains("Score: 50/100", output);
    }

    [Fact]
    public void JsonWriter_ProducesValidJsonWithComputedProperties()
    {
        var output = new JsonReportWriter().Write(SampleReport);
        using var document = JsonDocument.Parse(output);

        Assert.Equal(50, document.RootElement.GetProperty("score").GetInt32());
        Assert.Equal("passed", document.RootElement.GetProperty("checks")[0].GetProperty("status").GetString());
    }
}
