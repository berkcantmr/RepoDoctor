using RepoDoctor.Scanning;
using RepoDoctor.Tests.Support;

namespace RepoDoctor.Tests.Scanning;

public sealed class RepositoryScannerTests
{
    [Fact]
    public void Scan_EmptyDirectory_ReturnsWarningsForEveryCheck()
    {
        using var repository = new TemporaryRepository();

        var report = new RepositoryScanner().Scan(repository.Path);

        Assert.Equal(10, report.Checks.Count);
        Assert.Equal(0, report.Score);
        Assert.All(report.Checks, check => Assert.Equal(CheckStatus.Warning, check.Status));
    }

    [Fact]
    public void Scan_CompleteRepository_ReturnsPerfectScore()
    {
        using var repository = new TemporaryRepository();
        repository.CreateFile("README.md");
        repository.CreateFile("LICENSE");
        repository.CreateFile("CONTRIBUTING.md");
        repository.CreateFile("CODE_OF_CONDUCT.md");
        repository.CreateFile(".gitignore");
        repository.CreateFile("SECURITY.md");
        repository.CreateFile("CHANGELOG.md");
        repository.CreateFile("tests/Sample.Tests/Sample.Tests.csproj");
        repository.CreateFile(".github/workflows/ci.yml");
        repository.CreateDirectory(".git");

        var report = new RepositoryScanner().Scan(repository.Path);

        Assert.Equal(100, report.Score);
        Assert.False(report.HasFindings);
        Assert.All(report.Checks, check => Assert.Equal(CheckStatus.Passed, check.Status));
    }

    [Fact]
    public void Scan_AcceptsAlternativeLicenseAndCiNames()
    {
        using var repository = new TemporaryRepository();
        repository.CreateFile("COPYING");
        repository.CreateFile(".gitlab-ci.yml");

        var report = new RepositoryScanner().Scan(repository.Path);

        Assert.Equal(CheckStatus.Passed, report.Checks.Single(check => check.Id == "license").Status);
        Assert.Equal(CheckStatus.Passed, report.Checks.Single(check => check.Id == "ci").Status);
    }
}
