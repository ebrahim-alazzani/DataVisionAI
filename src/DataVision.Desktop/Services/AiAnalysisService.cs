using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

public class AiAnalysisService
{
    private readonly HttpClient _httpClient = new();
    public string? GeminiApiKey { get; set; }

    public async Task<List<AiInsight>> GenerateInsightsAsync(DataTableProfile profile)
    {
        // 1. Try Gemini if API key is provided
        if (!string.IsNullOrWhiteSpace(GeminiApiKey))
        {
            try
            {
                var geminiInsights = await GenerateGeminiInsightsAsync(profile);
                if (geminiInsights.Count > 0)
                    return geminiInsights;
            }
            catch
            {
                // Fallback to local heuristic engine smoothly
            }
        }

        // 2. High-precision Local Heuristic Analysis (Offline & 100% Free)
        return await Task.Run(() => GenerateLocalInsights(profile));
    }

    private List<AiInsight> GenerateLocalInsights(DataTableProfile profile)
    {
        var list = new List<AiInsight>();

        // 1. Overall Dataset Health Insight
        int totalCells = profile.TotalRows * profile.TotalColumns;
        int totalMissing = profile.Columns.Sum(c => c.NullCount);
        double healthPercentage = totalCells > 0 ? Math.Round((1.0 - (double)totalMissing / totalCells) * 100.0, 1) : 100;

        list.Add(new AiInsight
        {
            Title = "جودة واكتمال البيانات الكلية",
            Description = $"يتضمن الملف {profile.TotalRows:N0} صفاً موزعة على {profile.TotalColumns} أعمدة. نسبة اكتمال البيانات تصل إلى {healthPercentage}%، مما يجعلها مهيأة بدرجة عالية للتحليلات الإحصائية الدقيقة.",
            Type = healthPercentage > 90 ? InsightType.Summary : InsightType.Warning,
            ImpactMetric = $"{healthPercentage}% اكتمال",
            Severity = healthPercentage > 90 ? "Success" : "Warning"
        });

        // 2. Numeric Pillars Analysis
        var numericCols = profile.Columns.Where(c => c.IsNumeric).ToList();
        if (numericCols.Count > 0)
        {
            var topSumCol = numericCols.OrderByDescending(c => c.SumValue ?? 0).First();
            list.Add(new AiInsight
            {
                Title = $"المؤشر المالي/الرقمي الأبرز: {topSumCol.Name}",
                Description = $"سجل العمود ({topSumCol.Name}) إجمالي قدره {topSumCol.SumValue:N0} بمتوسط {topSumCol.MeanValue:N2}. يتراوح المدى بين {topSumCol.MinValue:N0} (أدنى قيمة) و {topSumCol.MaxValue:N0} (أعلى قيمة)، مما يشير إلى تباين ملحوظ يتطلب التدقيق.",
                Type = InsightType.Trend,
                ImpactMetric = $"{topSumCol.SumValue:N0} المجموع",
                Severity = "Info"
            });
        }

        // 3. Category / Grouping Opportunities
        var categoryCols = profile.Columns.Where(c => !c.IsNumeric && c.UniqueCount is >= 2 and <= 50).ToList();
        if (categoryCols.Count > 0)
        {
            var primaryCategory = categoryCols.First();
            list.Add(new AiInsight
            {
                Title = $"محور التصنيف الاستراتيجي: {primaryCategory.Name}",
                Description = $"يحتوي العمود ({primaryCategory.Name}) على {primaryCategory.UniqueCount} تصنيفات رئيسية فريدة. يوصى باعتماد هذا البعد كمرشح أولي للمقارنة بين الأقسام والفروع لتحديد مراكز التميز التشغيلي.",
                Type = InsightType.Recommendation,
                ImpactMetric = $"{primaryCategory.UniqueCount} فئات",
                Severity = "Success"
            });
        }

        // 4. Data Hygiene Warnings (Columns with excessive nulls)
        var highNullCols = profile.Columns.Where(c => c.MissingPercentage > 25).ToList();
        if (highNullCols.Count > 0)
        {
            var worstCol = highNullCols.OrderByDescending(c => c.MissingPercentage).First();
            list.Add(new AiInsight
            {
                Title = $"تنبيه: خلايا فارغة في عمود {worstCol.Name}",
                Description = $"تصل نسبة البيانات المفقودة في هذا العمود إلى {worstCol.MissingPercentage}% ({worstCol.NullCount} خلية فارغة). قد يؤدي الاعتماد عليه دون معالجة مسبقة إلى تشويه نتائج التقارير النهائية.",
                Type = InsightType.Anomaly,
                ImpactMetric = $"{worstCol.MissingPercentage}% مفقود",
                Severity = "Danger"
            });
        }

        // 5. Strategic Executive Recommendation
        list.Add(new AiInsight
        {
            Title = "توصية قيادية لاتخاذ القرار",
            Description = "يوصى بربط البيانات الرقمية بالأبعاد الزمنية أو الفئوية لإنشاء مقارنات ربع سنوية تكشف نقاط الهدر أو فرص النمو غير المستغلة.",
            Type = InsightType.Recommendation,
            ImpactMetric = "فرصة نمو",
            Severity = "Success"
        });

        return list;
    }

    private async Task<List<AiInsight>> GenerateGeminiInsightsAsync(DataTableProfile profile)
    {
        var summaryPrompt = BuildPromptForProfile(profile);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={GeminiApiKey}";
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = summaryPrompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var jsonResp = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResp);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString();

        var insights = new List<AiInsight>();
        if (!string.IsNullOrEmpty(text))
        {
            using var insightDoc = JsonDocument.Parse(text);
            if (insightDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in insightDoc.RootElement.EnumerateArray())
                {
                    var title = el.TryGetProperty("title", out var tp) ? tp.GetString() ?? "" : "";
                    var desc = el.TryGetProperty("description", out var dp) ? dp.GetString() ?? "" : "";
                    var metric = el.TryGetProperty("metric", out var mp) ? mp.GetString() ?? "" : "";
                    var typeStr = el.TryGetProperty("type", out var typeP) ? typeP.GetString()?.ToLower() ?? "" : "";

                    var type = typeStr switch
                    {
                        "trend" => InsightType.Trend,
                        "anomaly" => InsightType.Anomaly,
                        "recommendation" => InsightType.Recommendation,
                        "warning" => InsightType.Warning,
                        _ => InsightType.Summary
                    };

                    insights.Add(new AiInsight
                    {
                        Title = title,
                        Description = desc,
                        ImpactMetric = metric,
                        Type = type
                    });
                }
            }
        }

        return insights;
    }

    public async Task<string> AskQuestionAsync(DataTableProfile profile, string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(GeminiApiKey))
        {
            return LocalAnswerQuestion(profile, userQuestion);
        }

        try
        {
            var prompt = $@"أنت كبير محللي البيانات وذكاء الأعمال. إليك مواصفات جدول البيانات وعينة منه:
اسم الملف: {profile.FileName}
إجمالي الصفوف: {profile.TotalRows}
الأعمدة وإحصائياتها:
{string.Join("\n", profile.Columns.Select(c => $"- {c.Name} ({c.DataType}): مفقود {c.MissingPercentage}%, فريد {c.UniqueCount}, مجموع {c.SumValue}, متوسط {c.MeanValue}"))}

سؤال المستخدم: {userQuestion}

أجب بدقة ومهنية باللغة العربية، موضحاً الأرقام والحقائق مع التوصيات المناسبة.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={GeminiApiKey}";
            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var jsonResp = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResp);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString() ?? "لم يتم استلام رد كافٍ.";
        }
        catch (Exception ex)
        {
            return $"تعذر الاتصال بخدمة الذكاء الاصطناعي: {ex.Message}\n\nالإجابة المحلية السريعة:\n{LocalAnswerQuestion(profile, userQuestion)}";
        }
    }

    private string LocalAnswerQuestion(DataTableProfile profile, string question)
    {
        var q = question.ToLower();
        var sb = new StringBuilder();

        if (q.Contains("عمود") || q.Contains("أعمدة") || q.Contains("columns"))
        {
            sb.AppendLine($"يتضمن الملف {profile.TotalColumns} أعمدة هي:");
            foreach (var col in profile.Columns)
            {
                sb.AppendLine($"• {col.Name} ({col.DataType}) - القيم المكتملة: {col.NonNullCount}");
            }
            return sb.ToString();
        }

        if (q.Contains("صف") || q.Contains("صفوف") || q.Contains("حجم") || q.Contains("rows"))
        {
            return $"إجمالي عدد الصفوف المسجلة في ورقة العمل ({profile.SheetName}) هو {profile.TotalRows:N0} صفاً بحجم ملف {profile.FileSizeFormatted}.";
        }

        if (q.Contains("مجموع") || q.Contains("أعلى") || q.Contains("متوسط"))
        {
            var numCols = profile.Columns.Where(c => c.IsNumeric).ToList();
            if (numCols.Count > 0)
            {
                sb.AppendLine("الإحصائيات الرقمية المكتشفة:");
                foreach (var c in numCols)
                {
                    sb.AppendLine($"• {c.Name}: المجموع = {c.SumValue:N0} | المتوسط = {c.MeanValue:N2} | الأدنى = {c.MinValue} | الأعلى = {c.MaxValue}");
                }
                return sb.ToString();
            }
        }

        return $"تم فحص السؤال بناءً على بيانات ({profile.FileName}). يحتوي الجدول على {profile.TotalRows:N0} صفاً و {profile.TotalColumns} عموداً. للحصول على تحليل مخصص ومفصل لأي استفسار دقيق، يمكنك تفعيل مفتاح Gemini المجاني في صفحة الإعدادات.";
    }

    private static string BuildPromptForProfile(DataTableProfile profile)
    {
        return $@"أنت خبير ذكاء أعمال واستراتيجي بيانات. قم بتحليل هذا الجدول وأخرج قائمة JSON فقط بالمصفوفة التالية دون أي نص إضافي:
[
  {{
    ""title"": ""عنوان الرؤية"",
    ""description"": ""شرح دقيق وتفصيلي للاتجاه أو النتيجة"",
    ""type"": ""trend"" أو ""anomaly"" أو ""recommendation"" أو ""summary"",
    ""metric"": ""رقم أو نسبة مختصرة""
  }}
]

بيانات الجدول:
اسم الملف: {profile.FileName}
عدد الصفوف: {profile.TotalRows}
الأعمدة:
{string.Join("\n", profile.Columns.Select(c => $"- {c.Name} (نوع: {c.DataType}, مفقود: {c.MissingPercentage}%, مجموع: {c.SumValue}, متوسط: {c.MeanValue}, عينات: {string.Join(", ", c.SampleValues.Take(3))})"))}";
    }
}
