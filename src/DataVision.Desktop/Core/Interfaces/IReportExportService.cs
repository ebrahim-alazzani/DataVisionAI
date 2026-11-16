using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Core.Interfaces;

/// <summary>
/// واجهة تصدير التقارير التنفيذية والملخصات الإحصائية إلى صيغ قياسية (HTML, Markdown).
/// Contract for exporting executive reports and statistical summaries.
/// </summary>
public interface IReportExportService
{
    /// <summary>
    /// يولد تقريراً تنفيذياً كاملاً بصيغة HTML جاهز للطباعة أو التقديم للإدارة.
    /// Exports a self-contained HTML executive report ready for printing or presentation.
    /// </summary>
    Task<string> ExportHtmlReportAsync(DataTableProfile profile, IEnumerable<ExecutiveInsight> insights, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// يولد ملخصاً تنفيذياً بصيغة Markdown.
    /// Exports an executive summary in Markdown format.
    /// </summary>
    Task<string> ExportMarkdownReportAsync(DataTableProfile profile, IEnumerable<ExecutiveInsight> insights, string outputPath, CancellationToken cancellationToken = default);
}
