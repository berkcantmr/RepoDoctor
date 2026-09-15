using System.Text;
using RepoDoctor.Scanning;

namespace RepoDoctor.Reporting;

public sealed class TextReportWriter : IReportWriter
{
    public string Write(RepositoryReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var output = new StringBuilder();
        output.AppendLine($"RepoDoctor report: {report.Path}");
        output.AppendLine();

        foreach (var check in report.Checks)
        {
            var symbol = check.Status == CheckStatus.Passed ? "[PASS]" : "[WARN]";
            output.AppendLine($"{symbol} {check.Name}: {check.Message}");
            if (check.Status == CheckStatus.Warning)
            {
                output.AppendLine($"       Next: {check.Recommendation}");
            }
        }

        output.AppendLine();
        output.Append($"Score: {report.Score}/100 ({report.PassedCount} passed, {report.WarningCount} warnings)");
        return output.ToString();
    }
}
