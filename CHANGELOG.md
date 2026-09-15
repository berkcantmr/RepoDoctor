# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-09-15

### Added

- Repository checks for essential community files, tests, CI, and Git initialization.
- Text and JSON report formats.
- Optional strict exit code for CI usage.
- Unit test suite and GitHub Actions workflow.
# 0.2.0

- Expand to 14 checks, configurable exclusions/disabled checks and score gates.
- Add Markdown reporting, safe file output and check discovery.
- Fix traversal pruning, symlink cycles, empty-file false positives and numeric format acceptance.
- Add cross-platform package smoke tests and manual GitHub release workflow.
- Preserve exit codes; default scores may change because four checks were added.
