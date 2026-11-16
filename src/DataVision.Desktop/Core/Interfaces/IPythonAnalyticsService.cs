using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataVision.Desktop.Core.Interfaces;

public class CorrelationPair
{
    public string Col1 { get; set; } = string.Empty;
    public string Col2 { get; set; } = string.Empty;
    public double Correlation { get; set; }
    public string Strength { get; set; } = string.Empty;
}

public class MlForecastResult
{
    public string TargetColumn { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty;
    public double SlopePerStep { get; set; }
    public double R2Accuracy { get; set; }
    public List<double> ForecastNextSteps { get; set; } = new();
}

public class ClusterGroup
{
    public int ClusterId { get; set; }
    public int RecordsCount { get; set; }
    public double Ratio { get; set; }
}

public class PythonAnalyticsResult
{
    public bool Success { get; set; }
    public string Engine { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Columns { get; set; }
    public List<string> ColumnNames { get; set; } = new();
    public List<CorrelationPair> Correlations { get; set; } = new();
    public MlForecastResult? MlForecast { get; set; }
    public List<ClusterGroup> Clusters { get; set; } = new();
    public List<string> InsightsSummary { get; set; } = new();
    public string? Error { get; set; }
}

public interface IPythonAnalyticsService
{
    bool IsPythonAvailable { get; }
    string PythonPath { get; }
    Task<PythonAnalyticsResult?> AnalyzeAsync(string filePath, CancellationToken cancellationToken = default);
}
