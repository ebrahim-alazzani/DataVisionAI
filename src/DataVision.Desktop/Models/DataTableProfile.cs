using System;
using System.Collections.Generic;
using System.Data;

namespace DataVision.Desktop.Models;

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
    
    // In-memory DataTable for querying & preview
    public DataTable? RawTable { get; set; }

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
