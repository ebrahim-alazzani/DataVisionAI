using System;
using System.Collections.Generic;
using DataVision.Desktop.Core.Enums;

namespace DataVision.Desktop.Models;

/// <summary>
/// يمثل التوصيف الإحصائي والتحليلي الكامل لعمود في جدول البيانات.
/// Comprehensive statistical and demographic profile of a dataset column.
/// Follows SOLID: Single Responsibility Principle (pure data contract).
/// </summary>
public class DataColumnProfile
{
    /// <summary>اسم العمود / Column Name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>نوع البيانات المكتشف / Detected Data Type</summary>
    public ColumnDataType DataType { get; set; } = ColumnDataType.Text;

    /// <summary>الاسم النصي للنوع / String display of type</summary>
    public string DataTypeDisplay => DataType switch
    {
        ColumnDataType.Numeric => "رقمي (Numeric)",
        ColumnDataType.DateTime => "تاريخ/وقت (DateTime)",
        ColumnDataType.Boolean => "منطقي (Boolean)",
        _ => "نصي (Text)"
    };

    /// <summary>عدد الصفوف غير الفارغة / Count of non-empty cells</summary>
    public int NonNullCount { get; set; }

    /// <summary>عدد الخلايا الفارغة / Count of missing cells</summary>
    public int NullCount { get; set; }

    /// <summary>عدد القيم الفريدة / Unique distinct values</summary>
    public int UniqueCount { get; set; }

    // ==================== Descriptive Statistics (Numeric Only) ====================

    /// <summary>المجموع الكلي / Sum</summary>
    public double? SumValue { get; set; }

    /// <summary>المتوسط الحسابي / Arithmetic Mean</summary>
    public double? MeanValue { get; set; }

    /// <summary>الوسيط الإحصائي / Median</summary>
    public double? MedianValue { get; set; }

    /// <summary>أدنى قيمة / Minimum</summary>
    public double? MinValue { get; set; }

    /// <summary>أعلى قيمة / Maximum</summary>
    public double? MaxValue { get; set; }

    /// <summary>الانحراف المعياري / Standard Deviation (σ)</summary>
    public double? StandardDeviation { get; set; }

    /// <summary>التباين الإحصائي / Variance (σ²)</summary>
    public double? Variance { get; set; }

    /// <summary>الربيع الأول (مئوية 25%) / First Quartile (Q1)</summary>
    public double? Q1 { get; set; }

    /// <summary>الربيع الثالث (مئوية 75%) / Third Quartile (Q3)</summary>
    public double? Q3 { get; set; }

    /// <summary>المدى الربيعي / Interquartile Range (IQR = Q3 - Q1)</summary>
    public double? IQR => (Q3.HasValue && Q1.HasValue) ? Math.Round(Q3.Value - Q1.Value, 2) : null;

    /// <summary>عدد القيم الشاذة المكتشفة باختبار Tukey / Outliers Count</summary>
    public int OutliersCount { get; set; }

    // ==================== Samples and Helpers ====================

    /// <summary>عينات من القيم الموجودة / Sample values</summary>
    public List<string> SampleValues { get; set; } = new();

    /// <summary>هل العمود رقمي صالح للعمليات الرياضية؟</summary>
    public bool IsNumeric => DataType == ColumnDataType.Numeric;

    /// <summary>هل العمود زمني صالح لتحليل السلاسل الزمنية؟</summary>
    public bool IsDate => DataType == ColumnDataType.DateTime;

    /// <summary>نسبة الفراغ المئوية</summary>
    public double MissingPercentage => (NonNullCount + NullCount) == 0 ? 0 : Math.Round((double)NullCount / (NonNullCount + NullCount) * 100.0, 1);

    /// <summary>نسبة الاكتمال</summary>
    public double CompletenessPercentage => Math.Round(100.0 - MissingPercentage, 1);

    /// <summary>أول عينة بأمان تام دون أخطاء IndexOutOfRange</summary>
    public string FirstSample => SampleValues.Count > 0 ? SampleValues[0] : "-";
}
