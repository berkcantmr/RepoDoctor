# RepoDoctor contributor instructions

## Scope

These instructions apply to the entire repository.

## Development

- Target .NET 8 and keep the CLI cross-platform.
- Prefer .NET base-class-library APIs over new dependencies.
- Keep scanning logic independent from console input and output.
- Add or update tests for every behavior change.
- Preserve exit codes: `0` success, `1` findings in strict mode, `2` invalid usage.

## Validation

Run before submitting changes:

```bash
dotnet build RepoDoctor.sln --configuration Release
dotnet test RepoDoctor.sln --configuration Release --no-build
```
