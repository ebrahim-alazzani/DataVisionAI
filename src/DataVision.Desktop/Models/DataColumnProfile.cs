using System;
using System.Collections.Generic;

namespace DataVision.Desktop.Models;

public class DataColumnProfile
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public int NonNullCount { get; set; }
    public int NullCount { get; set; }
    public int UniqueCount { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public double? MeanValue { get; set; }
    public double? SumValue { get; set; }
    public List<string> SampleValues { get; set; } = new();

    public bool IsNumeric => DataType is "number" or "integer" or "decimal";
    public bool IsDate => DataType is "datetime" or "date";
    public double MissingPercentage => (NonNullCount + NullCount) == 0 ? 0 : Math.Round((double)NullCount / (NonNullCount + NullCount) * 100.0, 1);
    public string FirstSample => SampleValues.Count > 0 ? SampleValues[0] : "-";
}
