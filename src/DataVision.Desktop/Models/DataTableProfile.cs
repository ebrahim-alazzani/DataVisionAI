using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataVision.Desktop.Models;

/// <summary>
/// يمثل ملف التوصيف الشامل لجدول البيانات بعد فحصه إحصائياً.
/// Comprehensive demographic profile of a dataset.
/// </summary>
public class DataTableProfile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string SheetName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;

    public List<DataColumnProfile> Columns { get; set; } = new();
    public List<string> SheetNames { get; set; } = new();

    /// <summary>جدول البيانات الفعلي المحمل في الذاكرة للعمليات التحليلية والمخططات</summary>
    public DataTable? RawTable { get; set; }

    /// <summary>إجمالي الخلايا في الجدول</summary>
    public int TotalCells => TotalRows * TotalColumns;

    /// <summary>إجمالي الخلايا المفقودة</summary>
    public int TotalMissingCells => Columns.Sum(c => c.NullCount);

    /// <summary>مؤشر جودة واكتمال البيانات الكلي (0% إلى 100%)</summary>
    public double OverallQualityScore => TotalCells > 0 ? Math.Round((1.0 - (double)TotalMissingCells / TotalCells) * 100.0, 1) : 100.0;

    /// <summary>حجم الملف المنسق للقراءة</summary>
    public string FileSizeFormatted
    {
        get
        {
            if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
