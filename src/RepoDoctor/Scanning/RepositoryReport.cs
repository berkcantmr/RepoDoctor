namespace RepoDoctor.Scanning;

public sealed record RepositoryReport(string Path, IReadOnlyList<RepositoryCheck> Checks)
{
    public int PassedCount => Checks.Count(check => check.Status == CheckStatus.Passed);

    public int WarningCount => Checks.Count - PassedCount;

    public bool HasFindings => WarningCount > 0;

    public int Score => Checks.Count == 0
        ? 0
        : (int)Math.Round(PassedCount * 100d / Checks.Count, MidpointRounding.AwayFromZero);
}
