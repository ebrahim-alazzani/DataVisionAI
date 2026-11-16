using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Core.Enums;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

/// <summary>
/// محرك التحليل الإحصائي وذكاء الأعمال التنفيذي الخوارزمي (100% محلي وبدون أي اعتماديات خارجية).
/// Implements IAnalyticsEngineService using deterministic mathematical and statistical algorithms.
/// Follows SOLID: Single Responsibility Principle & Open/Closed Principle.
/// </summary>
public class StatisticalAnalyticsService : IAnalyticsEngineService
{
    /// <inheritdoc/>
    public async Task<List<ExecutiveInsight>> GenerateExecutiveInsightsAsync(DataTableProfile profile, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var insights = new List<ExecutiveInsight>();

            // 1. Data Integrity & Completeness Assessment
            GenerateDataQualityInsight(profile, insights);

            // 2. Dominant Metrics & Statistical Dispersion
            GenerateNumericPillarsInsight(profile, insights);

            // 3. Outliers & Anomalies (Tukey's IQR Test)
            GenerateOutlierInsights(profile, insights);

            // 4. Categorical Breakdown & Pareto 80/20 Rule
            GenerateCategoricalInsights(profile, insights);

            // 5. Strategic Executive Recommendation
            GenerateExecutiveSummaryInsight(profile, insights);

            return insights;
        }, cancellationToken);
    }

    private static void GenerateDataQualityInsight(DataTableProfile profile, List<ExecutiveInsight> insights)
    {
        double quality = profile.OverallQualityScore;
        var severity = quality >= 95 ? InsightSeverity.Success : (quality >= 80 ? InsightSeverity.Warning : InsightSeverity.Critical);

        insights.Add(new ExecutiveInsight
        {
            Title = "مؤشر سلامة واكتمال سجلات البيانات",
            Description = $"تم فحص {profile.TotalRows:N0} سجلاً و {profile.TotalColumns} أعمدة بإجمالي {profile.TotalCells:N0} خلية. بلغت نسبة الاكتمال {quality}% مع وجود {profile.TotalMissingCells:N0} خلية فارغة.",
            Category = InsightCategory.DataQuality,
            Severity = severity,
            ImpactMetric = $"{quality}% اكتمال",
            Confidence = 1.0
        });

        // Check for columns with severe nulls (>20%)
        foreach (var col in profile.Columns.Where(c => c.MissingPercentage > 20))
        {
            insights.Add(new ExecutiveInsight
            {
                Title = $"تنبيه جودة: فجوات بيانية في عمود ({col.Name})",
                Description = $"يحتوي العمود على نسبة خلايا فارغة قدرها {col.MissingPercentage}% ({col.NullCount:N0} خلية). يوصى بالتحقق من مسار التغذية أو استبعاد هذا البعد من المؤشرات الحساسة.",
                Category = InsightCategory.Anomaly,
                Severity = InsightSeverity.Warning,
                ImpactMetric = $"{col.MissingPercentage}% مفقود",
                Confidence = 1.0
            });
        }
    }

    private static void GenerateNumericPillarsInsight(DataTableProfile profile, List<ExecutiveInsight> insights)
    {
        var numericCols = profile.Columns.Where(c => c.IsNumeric && c.SumValue.HasValue).ToList();
        if (numericCols.Count == 0) return;

        var primaryCol = numericCols.OrderByDescending(c => c.SumValue ?? 0).First();

        double cv = (primaryCol.MeanValue.HasValue && primaryCol.MeanValue.Value > 0 && primaryCol.StandardDeviation.HasValue)
            ? Math.Round((primaryCol.StandardDeviation.Value / primaryCol.MeanValue.Value) * 100.0, 1)
            : 0;

        string dispersionDesc = cv > 50 ? "تبايناً وتذبذباً مرتفعاً (High Dispersion)" : "استقراراً وتجانساً متوازناً (Low Dispersion)";

        insights.Add(new ExecutiveInsight
        {
            Title = $"المؤشر القيادي الرئيسي: {primaryCol.Name}",
            Description = $"سجل العمود إجمالي تراكمي قدره {primaryCol.SumValue:N0} بمتوسط حسابي {primaryCol.MeanValue:N2} ووسيط {primaryCol.MedianValue:N2}. يظهر معامل التشتت ({cv}%) {dispersionDesc} عبر مختلف السجلات.",
            Category = InsightCategory.Trend,
            Severity = InsightSeverity.Info,
            ImpactMetric = $"{primaryCol.SumValue:N0} المجموع",
            Confidence = 1.0
        });
    }

    private static void GenerateOutlierInsights(DataTableProfile profile, List<ExecutiveInsight> insights)
    {
        foreach (var col in profile.Columns.Where(c => c.IsNumeric && c.OutliersCount > 0))
        {
            insights.Add(new ExecutiveInsight
            {
                Title = $"اكتشاف قيم شاذة إحصائياً في ({col.Name})",
                Description = $"أظهر اختبار Tukey للتباعد الربيعي (IQR = {col.IQR:N2}) وجود {col.OutliersCount} قيمة شاذة تتجاوز الحدود الطبيعية للتوزيع (أعلى من {col.Q3:N2} أو أقل من {col.Q1:N2}).",
                Category = InsightCategory.Anomaly,
                Severity = InsightSeverity.Critical,
                ImpactMetric = $"{col.OutliersCount} شاذة",
                Confidence = 0.99
            });
        }
    }

    private static void GenerateCategoricalInsights(DataTableProfile profile, List<ExecutiveInsight> insights)
    {
        if (profile.RawTable == null) return;

        var catCol = profile.Columns.FirstOrDefault(c => !c.IsNumeric && c.UniqueCount is >= 2 and <= 40);
        var valCol = profile.Columns.FirstOrDefault(c => c.IsNumeric && c.SumValue > 0);

        if (catCol == null || valCol == null) return;

        var aggregates = new Dictionary<string, double>();
        foreach (DataRow row in profile.RawTable.Rows)
        {
            var key = row[catCol.Name]?.ToString()?.Trim() ?? "أخرى";
            if (double.TryParse(row[valCol.Name]?.ToString()?.Trim(), out double numVal))
            {
                if (!aggregates.ContainsKey(key))
                    aggregates[key] = 0;
                aggregates[key] += numVal;
            }
        }

        if (aggregates.Count > 0)
        {
            var sorted = aggregates.OrderByDescending(kv => kv.Value).ToList();
            var top = sorted.First();
            double total = aggregates.Values.Sum();
            double topShare = total > 0 ? Math.Round((top.Value / total) * 100.0, 1) : 0;

            insights.Add(new ExecutiveInsight
            {
                Title = $"الريادة القطاعية: فئة ({top.Key})",
                Description = $"استحوذت الفئة ({top.Key}) وحدها على نسبة {topShare}% من إجمالي ({valCol.Name}) بقيمة إجمالية بلغت {top.Value:N0} من أصل {total:N0} موزعة على {aggregates.Count} فئات.",
                Category = InsightCategory.Distribution,
                Severity = InsightSeverity.Success,
                ImpactMetric = $"{topShare}% حصة",
                Confidence = 1.0
            });
        }
    }

    private static void GenerateExecutiveSummaryInsight(DataTableProfile profile, List<ExecutiveInsight> insights)
    {
        insights.Add(new ExecutiveInsight
        {
            Title = "التوصية الإدارية للتحسين التشغيلي",
            Description = "يوصى بتكثيف المتابعة على الفئات القيادية مع معالجة الأعمدة ذات الانحراف المرتفع لضمان دقة اتخاذ القرار وإحكام الرقابة على النتائج الدورية.",
            Category = InsightCategory.Recommendation,
            Severity = InsightSeverity.Success,
            ImpactMetric = "فرصة نمو",
            Confidence = 0.98
        });
    }

    /// <inheritdoc/>
    public async Task<string> ExecuteStatisticalQueryAsync(DataTableProfile profile, string query, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var q = query.Trim().ToLower();
            var sb = new StringBuilder();

            if (q.Contains("عمود") || q.Contains("أعمدة") || q.Contains("columns") || q.Contains("هيكل"))
            {
                sb.AppendLine($"📊 الهيكل الإحصائي للأعمدة ({profile.TotalColumns} أعمدة مسجلة):");
                sb.AppendLine("──────────────────────────────────────────");
                foreach (var col in profile.Columns)
                {
                    sb.AppendLine($"• {col.Name} ({col.DataTypeDisplay}) | اكتمال: {col.CompletenessPercentage}% | قيم فريدة: {col.UniqueCount:N0}");
                }
                return sb.ToString();
            }

            if (q.Contains("شاذ") || q.Contains("أخطاء") || q.Contains("outlier") || q.Contains("فارغ"))
            {
                sb.AppendLine("🔍 فحص القيم الشاذة والخلايا الفارغة:");
                sb.AppendLine("──────────────────────────────────────────");
                var outlierCols = profile.Columns.Where(c => c.OutliersCount > 0).ToList();
                if (outlierCols.Count > 0)
                {
                    foreach (var c in outlierCols)
                    {
                        sb.AppendLine($"⚠️ {c.Name}: تم اكتشاف {c.OutliersCount} قيمة شاذة إحصائياً (المدى الربيعي IQR = {c.IQR:N2})");
                    }
                }
                else
                {
                    sb.AppendLine("✅ لم يتم تسجيل أي قيم شاذة متطرفة في الأعمدة الرقمية.");
                }

                sb.AppendLine($"\nمؤشر جودة واكتمال الجدول الإجمالي: {profile.OverallQualityScore}% ({profile.TotalMissingCells:N0} خلية مفقودة).");
                return sb.ToString();
            }

            if (q.Contains("مجموع") || q.Contains("متوسط") || q.Contains("أعلى") || q.Contains("أدنى") || q.Contains("أرقام") || q.Contains("stats"))
            {
                var numCols = profile.Columns.Where(c => c.IsNumeric).ToList();
                sb.AppendLine("📈 المؤشرات الإحصائية للأعمدة الرقمية:");
                sb.AppendLine("──────────────────────────────────────────");
                foreach (var c in numCols)
                {
                    sb.AppendLine($"• {c.Name}:");
                    sb.AppendLine($"   المجموع: {c.SumValue:N0} | المتوسط: {c.MeanValue:N2} | الوسيط: {c.MedianValue:N2}");
                    sb.AppendLine($"   الأدنى: {c.MinValue:N0} | الأعلى: {c.MaxValue:N0} | الانحراف المعياري: {c.StandardDeviation:N2}");
                }
                return sb.ToString();
            }

            // Default Executive Snapshot
            sb.AppendLine($"📋 الملخص التنفيذي لملف ({profile.FileName}):");
            sb.AppendLine("──────────────────────────────────────────");
            sb.AppendLine($"• إجمالي السجلات: {profile.TotalRows:N0} صفاً في ورقة العمل ({profile.SheetName}).");
            sb.AppendLine($"• عدد الأعمدة: {profile.TotalColumns} أعمدة تشمل مقاييس رقمية وأبعاد تصنيفية.");
            sb.AppendLine($"• درجة جودة البيانات: {profile.OverallQualityScore}% مع جاهزية كاملة للتقارير التنفيذية.");
            sb.AppendLine("\n💡 يمكنك الاستفسار عن: (الأعمدة، المؤشرات الإحصائية، القيم الشاذة، أو ترتيب الفئات).");

            return sb.ToString();
        }, cancellationToken);
    }
}
