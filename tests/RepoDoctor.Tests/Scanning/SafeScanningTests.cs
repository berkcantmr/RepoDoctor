using RepoDoctor.Scanning;
using RepoDoctor.Tests.Support;

namespace RepoDoctor.Tests.Scanning;

public sealed class SafeScanningTests
{
    [Fact]
    public void EmptyFilesAndEmptyTestDirectoriesDoNotPass()
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile("README.md", "");
        repo.CreateDirectory("tests");
        var report = new RepositoryScanner().Scan(repo.Path);
        Assert.Equal(CheckStatus.Warning, report.Checks.Single(c => c.Id == "readme").Status);
        Assert.Equal(CheckStatus.Warning, report.Checks.Single(c => c.Id == "tests").Status);
    }

    [Theory]
    [InlineData("node_modules")]
    [InlineData(".git")]
    [InlineData("bin")]
    [InlineData("vendor")]
    [InlineData("custom")]
    public void ExcludedDirectoriesAreNotScanned(string directory)
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile($"{directory}/Sample.Tests.csproj");
        var report = new RepositoryScanner().Scan(repo.Path, new ScanConfiguration { ExcludeDirectories = ["custom"] });
        Assert.Equal(CheckStatus.Warning, report.Checks.Single(c => c.Id == "tests").Status);
    }

    [Theory]
    [InlineData("tests/sample.py")]
    [InlineData("src/example_test.go")]
    [InlineData("src/example.test.ts")]
    [InlineData("test_sample.py")]
    public void RecognizesMultipleTestConventions(string path)
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile(path);
        Assert.Equal(CheckStatus.Passed, new RepositoryScanner().Scan(repo.Path).Checks.Single(c => c.Id == "tests").Status);
    }

    [Fact]
    public void DisabledChecksAreRemovedFromScore()
    {
        using var repo = new TemporaryRepository();
        repo.CreateFile("README.md");
        var report = new RepositoryScanner().Scan(repo.Path, new ScanConfiguration
        { DisabledChecks = RepositoryScanner.CheckIds.Where(id => id != "readme").ToArray() });
        Assert.Single(report.Checks);
        Assert.Equal(100, report.Score);
    }

    [Fact]
    public void SymlinkedDirectoriesAreNotFollowed()
    {
        if (OperatingSystem.IsWindows()) return; // Windows symlink privilege is not guaranteed.
        using var repo = new TemporaryRepository();
        using var outside = new TemporaryRepository();
        outside.CreateFile("External.Tests.csproj");
        Directory.CreateSymbolicLink(Path.Combine(repo.Path, "linked"), outside.Path);
        Directory.CreateSymbolicLink(Path.Combine(repo.Path, "cycle"), repo.Path);
        File.CreateSymbolicLink(Path.Combine(repo.Path, "README.md"), Path.Combine(outside.Path, "External.Tests.csproj"));
        var report = new RepositoryScanner().Scan(repo.Path);
        Assert.Equal(CheckStatus.Warning, report.Checks.Single(c => c.Id == "tests").Status);
        Assert.Equal(CheckStatus.Warning, report.Checks.Single(c => c.Id == "readme").Status);
    }
}
