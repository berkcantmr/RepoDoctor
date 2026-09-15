using RepoDoctor.Cli;

namespace RepoDoctor.Tests.Cli;

public sealed class CliParserTests
{
    [Fact]
    public void Parse_NoArguments_UsesCurrentDirectoryAndTextOutput()
    {
        var result = CliParser.Parse([]);

        Assert.True(result.IsSuccess);
        Assert.Equal(".", result.Options!.Path);
        Assert.Equal(OutputFormat.Text, result.Options.Format);
        Assert.False(result.Options.Strict);
    }

    [Fact]
    public void Parse_ScanWithOptions_ReturnsExpectedOptions()
    {
        var result = CliParser.Parse(["scan", "./sample", "--format", "json", "--strict"]);

        Assert.True(result.IsSuccess);
        Assert.Equal("./sample", result.Options!.Path);
        Assert.Equal(OutputFormat.Json, result.Options.Format);
        Assert.True(result.Options.Strict);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("--format")]
    [InlineData("--format", "xml")]
    public void Parse_InvalidArguments_ReturnsError(params string[] arguments)
    {
        var result = CliParser.Parse(arguments);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }
}
