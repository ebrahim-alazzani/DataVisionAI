namespace DataVision.Desktop.Core.Enums;

/// <summary>
/// يحدد تصنيف الرؤية الإحصائية أو التنفيذية المستخرجة من البيانات.
/// Identifies the category of the statistical/business insight.
/// </summary>
public enum InsightCategory
{
    Trend,
    Anomaly,
    Summary,
    Recommendation,
    DataQuality,
    Distribution
}

/// <summary>
/// يحدد مستوى أهمية أو خطورة الرؤية الإحصائية.
/// Identifies the severity or importance level of the insight.
/// </summary>
public enum InsightSeverity
{
    Info,
    Success,
    Warning,
    Critical
}
