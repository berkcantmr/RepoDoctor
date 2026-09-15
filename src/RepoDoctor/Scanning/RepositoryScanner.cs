namespace RepoDoctor.Scanning;

public sealed class RepositoryScanner
{
    private static readonly FileRule[] FileRules =
    [
        new("readme", "README", ["README.md", "README.rst", "README.txt", "README"], "Add a README that explains the project's purpose, installation, and usage."),
        new("license", "License", ["LICENSE", "LICENSE.md", "LICENSE.txt", "COPYING"], "Choose an open-source license and add it to the repository root."),
        new("contributing", "Contribution guide", ["CONTRIBUTING.md", ".github/CONTRIBUTING.md"], "Add CONTRIBUTING.md with setup, test, and pull-request instructions."),
        new("code-of-conduct", "Code of conduct", ["CODE_OF_CONDUCT.md", ".github/CODE_OF_CONDUCT.md"], "Add a code of conduct and an enforcement contact."),
        new("gitignore", ".gitignore", [".gitignore"], "Add a .gitignore appropriate for the project's technology stack."),
        new("security", "Security policy", ["SECURITY.md", ".github/SECURITY.md"], "Document how security vulnerabilities should be reported."),
        new("changelog", "Changelog", ["CHANGELOG.md", "CHANGES.md", "HISTORY.md"], "Track notable changes in a changelog.")
    ];

    public RepositoryReport Scan(string repositoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var fullPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Repository directory was not found: {fullPath}");
        }

        var checks = new List<RepositoryCheck>();
        checks.AddRange(FileRules.Select(rule => EvaluateFileRule(fullPath, rule)));
        checks.Add(EvaluateTests(fullPath));
        checks.Add(EvaluateContinuousIntegration(fullPath));
        checks.Add(EvaluateGitRepository(fullPath));

        return new RepositoryReport(fullPath, checks);
    }

    private static RepositoryCheck EvaluateFileRule(string root, FileRule rule)
    {
        var match = rule.Candidates.FirstOrDefault(candidate => File.Exists(ToSystemPath(root, candidate)));
        return match is not null
            ? Passed(rule.Id, rule.Name, $"Found {match}.")
            : Warning(rule.Id, rule.Name, $"Missing {rule.Name.ToLowerInvariant()}.", rule.Recommendation);
    }

    private static RepositoryCheck EvaluateTests(string root)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", "bin", "obj", "node_modules" };
        var hasTestProject = EnumerateDirectoriesSafely(root)
            .Where(directory => !ContainsExcludedSegment(root, directory, excluded))
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*.*proj", SearchOption.TopDirectoryOnly))
            .Any(file => Path.GetFileNameWithoutExtension(file).Contains("test", StringComparison.OrdinalIgnoreCase));

        var hasConventionalTestDirectory = new[] { "test", "tests", "spec", "specs" }
            .Any(directory => Directory.Exists(Path.Combine(root, directory)));

        return hasTestProject || hasConventionalTestDirectory
            ? Passed("tests", "Automated tests", "Found a test project or conventional test directory.")
            : Warning("tests", "Automated tests", "No test project or conventional test directory found.", "Add automated tests for the project's important behavior.");
    }

    private static RepositoryCheck EvaluateContinuousIntegration(string root)
    {
        var workflowDirectory = Path.Combine(root, ".github", "workflows");
        var hasGitHubWorkflow = Directory.Exists(workflowDirectory)
            && Directory.EnumerateFiles(workflowDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Any(file => Path.GetExtension(file) is ".yml" or ".yaml");

        var hasOtherCi = new[] { ".gitlab-ci.yml", "azure-pipelines.yml", ".circleci/config.yml", "Jenkinsfile" }
            .Any(candidate => File.Exists(ToSystemPath(root, candidate)));

        return hasGitHubWorkflow || hasOtherCi
            ? Passed("ci", "Continuous integration", "Found a CI configuration.")
            : Warning("ci", "Continuous integration", "No supported CI configuration found.", "Add a CI workflow that builds and tests every pull request.");
    }

    private static RepositoryCheck EvaluateGitRepository(string root) =>
        Directory.Exists(Path.Combine(root, ".git")) || File.Exists(Path.Combine(root, ".git"))
            ? Passed("git", "Git repository", "Found Git repository metadata.")
            : Warning("git", "Git repository", "The directory is not initialized as a Git repository.", "Run 'git init' in the repository root.");

    private static IEnumerable<string> EnumerateDirectoriesSafely(string root)
    {
        yield return root;

        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> children;
            try
            {
                children = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var child in children)
            {
                yield return child;
                pending.Push(child);
            }
        }
    }

    private static bool ContainsExcludedSegment(string root, string directory, HashSet<string> excluded)
    {
        var relative = Path.GetRelativePath(root, directory);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(excluded.Contains);
    }

    private static string ToSystemPath(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static RepositoryCheck Passed(string id, string name, string message) =>
        new(id, name, CheckStatus.Passed, message, string.Empty);

    private static RepositoryCheck Warning(string id, string name, string message, string recommendation) =>
        new(id, name, CheckStatus.Warning, message, recommendation);

    private sealed record FileRule(string Id, string Name, string[] Candidates, string Recommendation);
}
