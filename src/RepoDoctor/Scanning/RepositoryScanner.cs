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
        new("changelog", "Changelog", ["CHANGELOG.md", "CHANGES.md", "HISTORY.md"], "Track notable changes in a changelog."),
        new("editorconfig", "Editor configuration", [".editorconfig"], "Add .editorconfig for consistent formatting."),
        new("pr-template", "Pull request template", [".github/pull_request_template.md", ".github/PULL_REQUEST_TEMPLATE.md", "PULL_REQUEST_TEMPLATE.md"], "Provide a pull request checklist."),
        new("dependency-updates", "Dependency updates", [".github/dependabot.yml", ".github/dependabot.yaml", "renovate.json", ".github/renovate.json", "renovate.json5"], "Configure Dependabot or Renovate updates.")
    ];

    public static IReadOnlyList<string> CheckIds { get; } = Array.AsReadOnly(FileRules.Select(rule => rule.Id)
        .Concat(["tests", "ci", "git", "issue-template"]).ToArray());

    public RepositoryReport Scan(string repositoryPath, ScanConfiguration? configuration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        configuration ??= new ScanConfiguration();
        configuration.Validate();

        var fullPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Repository directory was not found: {fullPath}");
        }

        var checks = new List<RepositoryCheck>();
        checks.AddRange(FileRules.Select(rule => EvaluateFileRule(fullPath, rule)));
        var files = EnumerateFilesSafely(fullPath, configuration.ExcludeDirectories).ToArray();
        checks.Add(EvaluateTests(fullPath, files));
        checks.Add(EvaluateContinuousIntegration(fullPath, files));
        checks.Add(EvaluateGitRepository(fullPath));
        var hasIssueTemplate = files.Any(file => file.StartsWith(".github/ISSUE_TEMPLATE/", StringComparison.OrdinalIgnoreCase)
            && !Path.GetFileName(file).Equals("config.yml", StringComparison.OrdinalIgnoreCase)
            && Path.GetExtension(file).ToLowerInvariant() is ".md" or ".yml" or ".yaml");
        checks.Add(hasIssueTemplate
            ? Passed("issue-template", "Issue templates", "Found an issue template.")
            : Warning("issue-template", "Issue templates", "No issue template found.", "Add a bug report or feature request template under .github/ISSUE_TEMPLATE/."));

        return new RepositoryReport(fullPath, checks.Where(check => !configuration.DisabledChecks.Contains(check.Id)).ToArray());
    }

    private static RepositoryCheck EvaluateFileRule(string root, FileRule rule)
    {
        var match = rule.Candidates.FirstOrDefault(candidate => IsNonEmptyRegularFile(root, ToSystemPath(root, candidate)));
        return match is not null
            ? Passed(rule.Id, rule.Name, $"Found {match}.")
            : Warning(rule.Id, rule.Name, $"Missing {rule.Name.ToLowerInvariant()}.", rule.Recommendation);
    }

    private static RepositoryCheck EvaluateTests(string root, string[] files)
    {
        var hasTestProject = files.Where(file => file.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
            .Any(file => Path.GetFileNameWithoutExtension(file).Contains("test", StringComparison.OrdinalIgnoreCase));

        var hasConventionalTestDirectory = files.Any(file => file.Split('/').SkipLast(1)
            .Any(segment => new[] { "test", "tests", "spec", "specs", "__tests__" }.Contains(segment, StringComparer.OrdinalIgnoreCase))
            && new[] { ".cs", ".fs", ".js", ".jsx", ".ts", ".tsx", ".py", ".go", ".rs", ".java", ".rb" }.Contains(Path.GetExtension(file)))
            || files.Any(file => Path.GetFileName(file).StartsWith("test_", StringComparison.OrdinalIgnoreCase) && file.EndsWith(".py"))
            || files.Any(file => file.EndsWith("_test.go") || file.EndsWith(".test.ts") || file.EndsWith(".test.js") || file.EndsWith(".spec.ts"));

        return hasTestProject || hasConventionalTestDirectory
            ? Passed("tests", "Automated tests", "Found a test project or conventional test directory.")
            : Warning("tests", "Automated tests", "No test project or conventional test directory found.", "Add automated tests for the project's important behavior.");
    }

    private static RepositoryCheck EvaluateContinuousIntegration(string root, string[] files)
    {
        var hasGitHubWorkflow = files.Any(file => file.StartsWith(".github/workflows/", StringComparison.Ordinal)
            && file.Split('/').Length == 3 && Path.GetExtension(file).ToLowerInvariant() is ".yml" or ".yaml"
            && IsNonEmptyRegularFile(root, ToSystemPath(root, file)));

        var hasOtherCi = new[] { ".gitlab-ci.yml", "azure-pipelines.yml", ".circleci/config.yml", "Jenkinsfile" }
            .Any(candidate => IsNonEmptyRegularFile(root, ToSystemPath(root, candidate)));

        return hasGitHubWorkflow || hasOtherCi
            ? Passed("ci", "Continuous integration", "Found a CI configuration.")
            : Warning("ci", "Continuous integration", "No supported CI configuration found.", "Add a CI workflow that builds and tests every pull request.");
    }

    private static RepositoryCheck EvaluateGitRepository(string root) =>
        Directory.Exists(Path.Combine(root, ".git")) || File.Exists(Path.Combine(root, ".git"))
            ? Passed("git", "Git repository", "Found Git repository metadata.")
            : Warning("git", "Git repository", "The directory is not initialized as a Git repository.", "Run 'git init' in the repository root.");

    private static bool IsNonEmptyRegularFile(string root, string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists || (file.Attributes & FileAttributes.ReparsePoint) != 0) return false;
        for (var parent = file.Directory; parent is not null && parent.FullName != Path.TrimEndingDirectorySeparator(root); parent = parent.Parent)
            if ((parent.Attributes & FileAttributes.ReparsePoint) != 0) return false;
        return file.Length > 0;
    }

    private static IEnumerable<string> EnumerateFilesSafely(string root, string[] additionalExclusions)
    {
        var excluded = new HashSet<string>(new[] { ".git", "bin", "obj", "node_modules", "vendor", ".venv", "venv", "dist", "coverage" }
            .Concat(additionalExclusions), StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>();
        pending.Push(root);
        var count = 0;
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            foreach (var entry in new DirectoryInfo(current).EnumerateFileSystemInfos().OrderBy(item => item.Name, StringComparer.Ordinal))
            {
                if (++count > 100_000) throw new IOException("Repository scan exceeded 100,000 entries. Configure excludeDirectories to narrow the scan.");
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                if (entry is DirectoryInfo)
                {
                    if (!excluded.Contains(entry.Name)) pending.Push(entry.FullName);
                }
                else yield return Path.GetRelativePath(root, entry.FullName).Replace(Path.DirectorySeparatorChar, '/');
            }
        }
    }

    private static string ToSystemPath(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static RepositoryCheck Passed(string id, string name, string message) =>
        new(id, name, CheckStatus.Passed, message, string.Empty);

    private static RepositoryCheck Warning(string id, string name, string message, string recommendation) =>
        new(id, name, CheckStatus.Warning, message, recommendation);

    private sealed record FileRule(string Id, string Name, string[] Candidates, string Recommendation);
}
