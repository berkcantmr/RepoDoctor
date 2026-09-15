# RepoDoctor v0.3.0

RepoDoctor can now be used directly in GitHub Actions workflows.

- Adds a root composite `action.yml` with path, format, strict, score, config, and output inputs.
- Passes inputs as process arguments without evaluating them as shell code.
- Tests the local Action on Linux, Windows, and macOS with a 100/100 self-audit.
- Retains the downloadable .NET tool package and the existing CLI behavior.

```yaml
- uses: actions/checkout@v4
- uses: berkcantmr/RepoDoctor@v0.3.0
  with:
    min-score: 80
```

The Action installs the repository-pinned .NET SDK and runs RepoDoctor from the tagged source. Pin the full `v0.3.0` tag for reproducible builds.
