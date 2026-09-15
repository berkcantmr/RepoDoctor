# Configuration and behavior

RepoDoctor automatically loads `.repodoctor.json` from the scanned directory. An explicit `--config` path is resolved from the current working directory and must exist. Unknown properties, unknown check IDs, null arrays and invalid scores fail with exit code 2.

```json
{
  "disabledChecks": ["git"],
  "excludeDirectories": ["generated", "fixtures"],
  "minScore": 80
}
```

`excludeDirectories` contains literal directory names, compared case-insensitively at every nesting level; paths and globs are not supported. Exclusions affect recursive discovery (tests, GitHub workflows and issue templates); explicitly named root/community files are still checked. Built-in excluded names: `.git`, `bin`, `obj`, `node_modules`, `vendor`, `.venv`, `venv`, `dist`, `coverage`.

`--min-score` overrides the configured threshold. `--strict` fails for any enabled warning independently of the score threshold. Disabled checks are removed from the denominator. With no enabled checks the score is 0, not 100.

Exit codes: 0 = scan completed and selected gates passed; 1 = strict/score gate failed; 2 = invalid arguments/configuration or filesystem failure. Without strict mode or a score threshold, findings alone do not cause a failing exit code.

The scanner is offline and does not run code, invoke Git, modify the scanned repository, check remote CI status, validate license text, parse workflow semantics or verify test quality. Detection is filename-based and heuristic. Non-empty means file length greater than zero, not a content-quality assessment. Symlinked files/directories inside the scan root are skipped. Permission and I/O errors fail visibly rather than producing a misleading complete report. Traversal stops with an error after 100,000 entries; narrow discovery with exclusions for larger repositories.

`--output` writes a new UTF-8 file relative to the current directory. Parent directories must exist. Existing files are never overwritten; choose a new filename or explicitly remove your old report before running again.

## Check IDs

| ID | Evidence |
| --- | --- |
| readme | Non-empty README.md/rst/txt or README |
| license | Non-empty LICENSE/md/txt or COPYING |
| contributing | Contribution guide in root or .github |
| code-of-conduct | Conduct guide in root or .github |
| gitignore | Root .gitignore |
| security | Security policy in root or .github |
| changelog | CHANGELOG.md, CHANGES.md or HISTORY.md |
| editorconfig | Root .editorconfig |
| pr-template | Root or .github pull-request template |
| dependency-updates | Dependabot or Renovate configuration |
| tests | Test-named project or recognized test source convention |
| ci | GitHub workflow, GitLab, Azure Pipelines, CircleCI or Jenkins configuration |
| git | Root .git directory or worktree metadata file |
| issue-template | Markdown/YAML issue template under .github/ISSUE_TEMPLATE |

File candidate casing is filesystem-dependent; the canonical names above are recommended. Issue/PR templates and test files are presence heuristics, not semantic validation.
