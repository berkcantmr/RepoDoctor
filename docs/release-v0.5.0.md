# RepoDoctor v0.5.0

This maintenance release refreshes RepoDoctor's GitHub Actions and test infrastructure.

## Changes

- Upgrade `actions/checkout` to v7.
- Upgrade `actions/setup-dotnet` to v6.
- Upgrade `actions/upload-artifact` to v7.
- Upgrade xUnit to 2.9.3, Coverlet Collector to 10.0.1 and Microsoft.NET.Test.Sdk to 18.10.1.
- Add the NuGet.org installation path and badge to the README.

## Compatibility

The updated GitHub Actions use Node.js 24. GitHub-hosted runners are supported. Users of self-hosted runners should update their runner before moving to `berkcantmr/RepoDoctor@v0.5.0`.

The RepoDoctor CLI continues to target .NET 8 and retains the existing command-line behavior and exit codes.
