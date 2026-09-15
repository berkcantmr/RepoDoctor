# RepoDoctor

[![CI](https://github.com/berkcantmr/RepoDoctor/actions/workflows/ci.yml/badge.svg)](https://github.com/berkcantmr/RepoDoctor/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/berkcantmr/RepoDoctor)](https://github.com/berkcantmr/RepoDoctor/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A cross-platform .NET command-line tool that checks whether a repository contains the essential files and automation expected from a healthy open-source project.

## Use in GitHub Actions

```yaml
- uses: actions/checkout@v4
- uses: berkcantmr/RepoDoctor@v0.4.0
  with:
    min-score: 80
```

The Action supports strict mode, JSON/Markdown reports, configuration files, and report artifacts. See the [complete GitHub Action guide](docs/github-action.md).

## Current checks

- README
- Open-source license
- Contribution guide
- Code of conduct
- `.gitignore`
- Security policy
- Changelog
- Automated tests
- Continuous integration
- Git repository initialization
- Editor configuration
- Pull request template
- Dependency update configuration
- Issue templates

Checks are offline filename-based heuristics, not a security audit or proof of project quality. [Configuration, check IDs and limitations](docs/configuration.md).

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run from source

```bash
git clone https://github.com/berkcantmr/RepoDoctor.git
cd RepoDoctor
dotnet run --project src/RepoDoctor -- scan .
```

Example output:

```text
RepoDoctor report: /projects/example

[PASS] README: Found README.md.
[WARN] License: Missing license.
       Next: Choose an open-source license and add it to the repository root.

Score: 50/100 (7 passed, 7 warnings)
```

## Commands

```bash
# Scan the current directory
dotnet run --project src/RepoDoctor -- scan .

# Produce machine-readable output
dotnet run --project src/RepoDoctor -- scan . --format json

# Return exit code 1 when findings exist (useful in CI)
dotnet run --project src/RepoDoctor -- scan . --strict

# Save a Markdown report to a new file
dotnet run --project src/RepoDoctor -- scan . --format markdown --output report.md

# Fail CI below a minimum readiness score
dotnet run --project src/RepoDoctor -- scan . --min-score 80

# Discover IDs usable in disabledChecks
dotnet run --project src/RepoDoctor -- --list-checks
```

## Build and test

```bash
dotnet restore RepoDoctor.sln
dotnet build RepoDoctor.sln --configuration Release --no-restore
dotnet test RepoDoctor.sln --configuration Release --no-build
```

## Install as a local .NET tool

```bash
dotnet pack src/RepoDoctor/RepoDoctor.csproj --configuration Release
dotnet tool install --global --add-source artifacts RepoDoctor.Tool --version 0.4.0
repodoctor scan .
```

## Configuration

Create `.repodoctor.json` in the scanned repository (optional):

```json
{
  "disabledChecks": ["git"],
  "excludeDirectories": ["generated"],
  "minScore": 80
}
```

Use `--config path/to/policy.json` for an explicit configuration. Command-line score thresholds override configuration. Exit codes: **0** success, **1** failed strict/score gate, **2** usage/configuration/I/O error. Scan results without a gate return 0 even with warnings.

## Releases

After successful main-branch CI, the **Release** workflow builds, tests and installs the exact tested commit before publishing `v0.4.0` with a downloadable `.nupkg`. Existing releases are never replaced. [Release notes](docs/release-v0.4.0.md).

NuGet.org publication uses a separate, manually triggered OIDC trusted-publishing workflow. It requests a short-lived credential only after rebuilding and testing the selected release tag; no permanent API key is stored. Until the first NuGet publication succeeds, install the downloaded/local package with `--add-source` as shown above.

CI runs on Linux, Windows and macOS and installs the actual package before self-scanning this repository. Scanning never executes code from the target repository.

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

## License

Licensed under the [MIT License](LICENSE).
