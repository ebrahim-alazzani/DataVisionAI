using DataVision.Desktop.Models;

namespace DataVision.Desktop.Core.Interfaces;

/// <summary>
/// واجهة محرك توليد الرسوم والمخططات البيانية التفاعلية.
/// Contract for generating interactive visualizations and HTML chart specs.
/// </summary>
public interface IChartGeneratorService
{
    /// <summary>
    /// يولد كود HTML لمخطط ApexCharts تفاعلي بناءً على الأبعاد والمتغيرات المحددة.
    /// Generates interactive ApexCharts HTML markup for the selected category and value dimensions.
    /// </summary>
    string GenerateChartHtml(
        DataTableProfile profile,
        string? categoryColName = null,
        string? valueColName = null,
        string chartType = "bar",
        bool isDark = true);
}
