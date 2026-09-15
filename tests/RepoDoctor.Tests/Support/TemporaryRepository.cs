namespace RepoDoctor.Tests.Support;

internal sealed class TemporaryRepository : IDisposable
{
    public TemporaryRepository()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"repodoctor-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void CreateFile(string relativePath, string content = "")
    {
        var path = System.IO.Path.Combine(Path, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        var directory = System.IO.Path.GetDirectoryName(path);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
    }

    public void CreateDirectory(string relativePath) =>
        Directory.CreateDirectory(System.IO.Path.Combine(Path, relativePath));

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
