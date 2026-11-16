using System;

namespace DataVision.Desktop.Models;

public enum InsightType
{
    Trend,
    Anomaly,
    Summary,
    Recommendation,
    Warning
}

public class AiInsight
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; } = InsightType.Summary;
    public string Severity { get; set; } = "Info"; // Info, Success, Warning, Danger
    public string ImpactMetric { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.95;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string IconGlyph => Type switch
    {
        InsightType.Trend => "\uE9D9",          // Trending
        InsightType.Anomaly => "\uE7BA",        // Warning
        InsightType.Recommendation => "\uE945", // Lightbulb
        InsightType.Warning => "\uE783",        // Alert
        _ => "\uE9F9"                           // Information
    };

    public string BadgeColor => Type switch
    {
        InsightType.Trend => "#0EA5E9",
        InsightType.Anomaly => "#F43F5E",
        InsightType.Recommendation => "#10B981",
        InsightType.Warning => "#F59E0B",
        _ => "#6366F1"
    };
}
