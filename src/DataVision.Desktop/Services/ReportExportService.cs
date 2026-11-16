using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

/// <summary>
/// خدمة تصدير التقارير التنفيذية بصيغ HTML و Markdown.
/// Implements IReportExportService following SOLID: Single Responsibility Principle.
/// </summary>
public class ReportExportService : IReportExportService
{
    /// <inheritdoc/>
    public async Task<string> ExportHtmlReportAsync(DataTableProfile profile, IEnumerable<ExecutiveInsight> insights, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine($"  <title>تقرير تنفيذي: {profile.FileName}</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: 'Segoe UI', Tahoma, sans-serif; background: #0B0F19; color: #E2E8F0; margin: 0; padding: 32px; }");
            sb.AppendLine("    .container { max-width: 960px; margin: auto; background: #131B2A; border-radius: 16px; padding: 32px; border: 1px solid #1E293B; }");
            sb.AppendLine("    h1 { color: #38BDF8; margin-top: 0; }");
            sb.AppendLine("    .badge { display: inline-block; padding: 4px 10px; border-radius: 6px; font-size: 12px; font-weight: bold; background: #0284C7; color: white; }");
            sb.AppendLine("    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin: 24px 0; }");
            sb.AppendLine("    .kpi-card { background: #0F172A; border: 1px solid #1E293B; border-radius: 12px; padding: 16px; text-align: center; }");
            sb.AppendLine("    .kpi-val { font-size: 24px; font-weight: bold; color: #38BDF8; margin: 8px 0; }");
            sb.AppendLine("    .kpi-label { font-size: 12px; color: #94A3B8; }");
            sb.AppendLine("    .insight-card { background: #0F172A; border-right: 4px solid #38BDF8; border-radius: 8px; padding: 16px; margin-bottom: 12px; }");
            sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-top: 24px; }");
            sb.AppendLine("    th, td { padding: 10px 14px; text-align: right; border-bottom: 1px solid #1E293B; font-size: 13px; }");
            sb.AppendLine("    th { background: #0F172A; color: #94A3B8; font-weight: bold; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class=\"container\">");
            sb.AppendLine($"    <h1>تقرير التحليل الإحصائي التنفيذي · DataVision Studio</h1>");
            sb.AppendLine($"    <p><span class=\"badge\">الملف: {profile.FileName}</span> <span class=\"badge\">الورقة: {profile.SheetName}</span> <span class=\"badge\">تاريخ التوليد: {profile.AnalyzedAt:yyyy-MM-dd HH:mm}</span></p>");

            sb.AppendLine("    <div class=\"kpi-grid\">");
            sb.AppendLine($"      <div class=\"kpi-card\"><div class=\"kpi-label\">إجمالي السجلات</div><div class=\"kpi-val\">{profile.TotalRows:N0}</div></div>");
            sb.AppendLine($"      <div class=\"kpi-card\"><div class=\"kpi-label\">عدد الأعمدة</div><div class=\"kpi-val\">{profile.TotalColumns}</div></div>");
            sb.AppendLine($"      <div class=\"kpi-card\"><div class=\"kpi-label\">مؤشر الاكتمال</div><div class=\"kpi-val\">{profile.OverallQualityScore}%</div></div>");
            sb.AppendLine($"      <div class=\"kpi-card\"><div class=\"kpi-label\">حجم الملف</div><div class=\"kpi-val\">{profile.FileSizeFormatted}</div></div>");
            sb.AppendLine("    </div>");

            sb.AppendLine("    <h2>الرؤى والقرارات التنفيذية:</h2>");
            foreach (var insight in insights)
            {
                sb.AppendLine("    <div class=\"insight-card\">");
                sb.AppendLine($"      <h3 style=\"margin:0 0 6px 0; color:#F1F5F9;\">{insight.Title} <span class=\"badge\" style=\"background:{insight.BadgeColor}; float:left;\">{insight.ImpactMetric}</span></h3>");
                sb.AppendLine($"      <p style=\"margin:0; color:#94A3B8; font-size:13px; line-height:1.6;\">{insight.Description}</p>");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("    <h2>هيكل الأعمدة والإحصائيات الوصفية:</h2>");
            sb.AppendLine("    <table>");
            sb.AppendLine("      <thead><tr><th>العمود</th><th>النوع</th><th>المكتمل</th><th>الفارغ</th><th>المتوسط / المجموع</th><th>الوسيط</th><th>IQR</th></tr></thead>");
            sb.AppendLine("      <tbody>");
            foreach (var col in profile.Columns)
            {
                string meanSum = col.IsNumeric ? $"{col.MeanValue:N2} / {col.SumValue:N0}" : "-";
                string median = col.IsNumeric ? $"{col.MedianValue:N2}" : "-";
                string iqr = col.IsNumeric ? $"{col.IQR:N2}" : "-";
                sb.AppendLine($"        <tr><td><b>{col.Name}</b></td><td>{col.DataTypeDisplay}</td><td>{col.NonNullCount:N0}</td><td>{col.NullCount:N0}</td><td>{meanSum}</td><td>{median}</td><td>{iqr}</td></tr>");
            }
            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");

            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            return outputPath;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> ExportMarkdownReportAsync(DataTableProfile profile, IEnumerable<ExecutiveInsight> insights, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# تقرير التحليل التنفيذي: {profile.FileName}");
            sb.AppendLine($"*تاريخ التوليد: {profile.AnalyzedAt:yyyy-MM-dd HH:mm} | المنصة: DataVision Studio*\n");

            sb.AppendLine("## 1. المؤشرات الرقمية الرئيسية (KPIs)");
            sb.AppendLine($"* **إجمالي السجلات (Rows):** {profile.TotalRows:N0}");
            sb.AppendLine($"* **عدد الأعمدة (Columns):** {profile.TotalColumns}");
            sb.AppendLine($"* **مستوى جودة البيانات:** {profile.OverallQualityScore}%");
            sb.AppendLine($"* **ورقة العمل الحالية:** {profile.SheetName} ({profile.FileSizeFormatted})\n");

            sb.AppendLine("## 2. الرؤى والتوصيات التنفيذية");
            foreach (var inSight in insights)
            {
                sb.AppendLine($"### {inSight.Title} [{inSight.ImpactMetric}]");
                sb.AppendLine($"{inSight.Description}\n");
            }

            sb.AppendLine("## 3. توصيف الأعمدة والإحصائيات");
            sb.AppendLine("| اسم العمود | النوع | القيم المكتملة | الفراغات | المتوسط | الوسيط | الانحراف المعياري | IQR |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");
            foreach (var col in profile.Columns)
            {
                string mean = col.IsNumeric ? $"{col.MeanValue:N2}" : "-";
                string median = col.IsNumeric ? $"{col.MedianValue:N2}" : "-";
                string std = col.IsNumeric ? $"{col.StandardDeviation:N2}" : "-";
                string iqr = col.IsNumeric ? $"{col.IQR:N2}" : "-";
                sb.AppendLine($"| {col.Name} | {col.DataTypeDisplay} | {col.NonNullCount:N0} | {col.NullCount:N0} | {mean} | {median} | {std} | {iqr} |");
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            return outputPath;
        }, cancellationToken);
    }
}
