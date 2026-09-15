# RepoDoctor

RepoDoctor is a small .NET command-line tool that checks whether a repository contains the essential files and automation expected from a healthy open-source project.

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

Score: 50/100 (5 passed, 5 warnings)
```

## Commands

```bash
# Scan the current directory
dotnet run --project src/RepoDoctor -- scan .

# Produce machine-readable output
dotnet run --project src/RepoDoctor -- scan . --format json

# Return exit code 1 when findings exist (useful in CI)
dotnet run --project src/RepoDoctor -- scan . --strict
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
dotnet tool install --global --add-source artifacts RepoDoctor.Tool
repodoctor scan .
```

## Roadmap

- Detect incomplete README sections
- Support a configurable policy file
- Add SARIF output for GitHub code scanning
- Publish the tool to NuGet

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request.

## License

Licensed under the [MIT License](LICENSE).
