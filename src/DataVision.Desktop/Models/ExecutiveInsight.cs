using System;
using DataVision.Desktop.Core.Enums;

namespace DataVision.Desktop.Models;

/// <summary>
/// يمثل رؤية أو تنبيهاً تنفيذياً محسوباً رياضياً من بيانات الجدول.
/// Represents a statistically derived executive business insight or anomaly.
/// </summary>
public class ExecutiveInsight
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>عنوان الرؤية التنفيذية / Title</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>الشرح والتفاصيل الإحصائية والتوصيات / Description & recommendation</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>تصنيف الرؤية (اتجاه، قيمة شاذة، جودة بيانات، إلخ) / Category</summary>
    public InsightCategory Category { get; set; } = InsightCategory.Summary;

    /// <summary>مستوى الأهمية / Severity level</summary>
    public InsightSeverity Severity { get; set; } = InsightSeverity.Info;

    /// <summary>المؤشر الرقمي المحسوب / Key impacted metric</summary>
    public string ImpactMetric { get; set; } = string.Empty;

    /// <summary>مستوى الثقة الإحصائية (0 إلى 1.0) / Statistical confidence</summary>
    public double Confidence { get; set; } = 1.0;

    /// <summary>تاريخ ووقت التحليل / Generated timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>رمز الأيقونة في نظام التصميم / UI Icon Glyph</summary>
    public string IconGlyph => Category switch
    {
        InsightCategory.Trend => "\uE9D9",           // Trending
        InsightCategory.Anomaly => "\uE7BA",         // Warning
        InsightCategory.Recommendation => "\uE945",  // Lightbulb
        InsightCategory.DataQuality => "\uE783",     // Alert
        InsightCategory.Distribution => "\uE9F9",    // Statistics
        _ => "\uE80F"                                // Info
    };

    /// <summary>لون الشارة في الواجهة / Badge accent brush hex</summary>
    public string BadgeColor => Severity switch
    {
        InsightSeverity.Critical => "#F43F5E",       // Rose Red
        InsightSeverity.Warning => "#F59E0B",        // Amber
        InsightSeverity.Success => "#10B981",        // Emerald Green
        _ => "#0EA5E9"                               // Cyan / Blue
    };
}
