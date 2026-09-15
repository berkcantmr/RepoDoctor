# RepoDoctor v0.4.0

This release prepares RepoDoctor for its first NuGet.org publication.

- Adds NuGet project, repository, copyright, search-tag, and release-note metadata.
- Adds a manually triggered trusted-publishing workflow with GitHub OIDC.
- Validates that the requested immutable tag matches the package version before publishing.
- Rebuilds and tests the tagged source before requesting a short-lived NuGet credential.
- Keeps the GitHub Action and CLI behavior introduced in v0.3.0.

After publication, install the tool from NuGet.org:

```sh
dotnet tool install --global RepoDoctor.Tool --version 0.4.0
repodoctor scan .
```

The NuGet workflow requires a nuget.org trusted-publishing policy for the `berkcantmr` account, `berkcantmr/RepoDoctor`, and `publish-nuget.yml`. It stores no long-lived API key.
