# GitHub Action

RepoDoctor can run as a composite action on GitHub-hosted Linux, Windows, and macOS runners. The action installs the .NET version pinned by this repository and runs the checked-in RepoDoctor source. It does not execute code from the repository being scanned.

## Basic workflow

```yaml
name: Repository readiness

on:
  pull_request:
  push:
    branches: [main]

permissions:
  contents: read

jobs:
  audit:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: berkcantmr/RepoDoctor@v0.4.0
        with:
          strict: true
```

Use an immutable release tag such as `v0.4.0` for reproducible builds. Review release notes before upgrading.

## Inputs

| Input | Default | Behavior |
| --- | --- | --- |
| `path` | `.` | Repository path relative to the GitHub workspace |
| `format` | `text` | `text`, `json`, or `markdown` |
| `strict` | `false` | Fails on any enabled warning when set exactly to `true` |
| `min-score` | empty | Fails below an integer score from 0 to 100 |
| `config` | empty | Explicit `.repodoctor.json` path |
| `output` | empty | Writes to a new file; never overwrites an existing file |

Paths are resolved from `${{ github.workspace }}`. Parent directories for `output` must already exist. When `output` is set, upload it in a later workflow step if it should be retained:

```yaml
- uses: berkcantmr/RepoDoctor@v0.4.0
  with:
    format: markdown
    min-score: 80
    output: repodoctor-report.md

- uses: actions/upload-artifact@v4
  if: always()
  with:
    name: repodoctor-report
    path: repodoctor-report.md
```

Exit code 1 fails the step when the selected gate is not met. Invalid input, configuration, or filesystem access returns exit code 2. See [configuration and limitations](configuration.md) for the full behavior contract.
