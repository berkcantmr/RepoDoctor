using RepoDoctor.Scanning;

namespace RepoDoctor.Reporting;

public interface IReportWriter
{
    string Write(RepositoryReport report);
}
