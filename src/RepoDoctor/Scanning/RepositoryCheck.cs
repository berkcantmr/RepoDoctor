namespace RepoDoctor.Scanning;

public sealed record RepositoryCheck(
    string Id,
    string Name,
    CheckStatus Status,
    string Message,
    string Recommendation);
