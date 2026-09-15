# RepoDoctor v0.2.0

RepoDoctor is a local, offline repository-readiness checker, not a security scanner or a certification service.

- 14 repository checks with actionable recommendations.
- JSON configuration: disable selected checks, exclude directory names, set a minimum score.
- Text, JSON and Markdown reports; optional no-overwrite output files.
- CI threshold, strict mode and discoverable check IDs.
- Pruned dependency/build directories, skipped symlinks and bounded traversal.
- Non-empty documentation and CI files required; empty test directories no longer pass.
- Linux, Windows and macOS CI with packaged-tool installation tests.

Download `RepoDoctor.Tool.0.2.0.nupkg` into a folder, then run:

```sh
dotnet tool install RepoDoctor.Tool --version 0.2.0 --global --add-source ./download-folder
repodoctor scan .
```

Requires the .NET 8 SDK for installation and .NET 8 runtime for execution. This release does not publish to NuGet.org automatically.

Compatibility: the default scan now includes 14 checks (previously 10), so scores and strict-mode outcomes may change. Existing check IDs and exit codes remain stable.
