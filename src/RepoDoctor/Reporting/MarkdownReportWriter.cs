using System.Text;
using RepoDoctor.Scanning;

namespace RepoDoctor.Reporting;

public sealed class MarkdownReportWriter : IReportWriter
{
    public string Write(RepositoryReport report)
    {
        var result = new StringBuilder();
        result.AppendLine("# RepoDoctor report");
        result.AppendLine();
        result.AppendLine($"Repository: {Escape(report.Path)}");
        result.AppendLine($"Score: {report.Score}/100 — {report.PassedCount} passed, {report.WarningCount} warnings");
        result.AppendLine();
        result.AppendLine("| Check | Status | Evidence | Recommendation |");
        result.AppendLine("| --- | --- | --- | --- |");
        foreach (var check in report.Checks)
            result.AppendLine($"| {Escape(check.Id)} | {check.Status} | {Escape(check.Message)} | {Escape(check.Recommendation)} |");
        return result.ToString().TrimEnd();
    }

    private static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
        .Replace("\\", "&#92;").Replace("|", "&#124;").Replace("`", "&#96;").Replace("\r", " ").Replace("\n", " ");
}
