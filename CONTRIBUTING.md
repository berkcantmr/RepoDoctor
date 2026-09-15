# Contributing to RepoDoctor

Thank you for considering a contribution.

## Development setup

1. Install the .NET 8 SDK.
2. Fork and clone the repository.
3. Create a branch from `main`.
4. Run `dotnet restore RepoDoctor.sln`.
5. Make a focused change and add tests for changed behavior.
6. Run the validation commands below.

```bash
dotnet build RepoDoctor.sln --configuration Release
dotnet test RepoDoctor.sln --configuration Release --no-build
```

## Pull requests

- Keep each pull request focused on one problem.
- Explain both the behavior change and the reason for it.
- Link the related issue when one exists.
- Update documentation when CLI behavior changes.
- Do not commit generated `bin`, `obj`, package, or test-result files.

By participating, you agree to follow the project's [Code of Conduct](CODE_OF_CONDUCT.md).
