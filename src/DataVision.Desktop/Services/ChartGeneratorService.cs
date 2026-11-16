using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

/// <summary>
/// خدمة توليد المخططات والرسوم البيانية التفاعلية باستخدام ApexCharts و HTML5.
/// Implements IChartGeneratorService following SOLID: Single Responsibility Principle.
/// </summary>
public class ChartGeneratorService : IChartGeneratorService
{
    /// <inheritdoc/>
    public string GenerateChartHtml(
        DataTableProfile profile,
        string? categoryColName = null,
        string? valueColName = null,
        string chartType = "bar",
        bool isDark = true)
    {
        var catCol = !string.IsNullOrEmpty(categoryColName)
            ? profile.Columns.FirstOrDefault(c => c.Name == categoryColName)
            : profile.Columns.FirstOrDefault(c => !c.IsNumeric && c.UniqueCount is >= 2 and <= 25);

        var valCol = !string.IsNullOrEmpty(valueColName)
            ? profile.Columns.FirstOrDefault(c => c.Name == valueColName)
            : profile.Columns.FirstOrDefault(c => c.IsNumeric);

        var categories = new List<string>();
        var values = new List<double>();

        if (profile.RawTable != null && catCol != null && valCol != null)
        {
            var groups = new Dictionary<string, double>();

            foreach (DataRow row in profile.RawTable.Rows)
            {
                var catVal = row[catCol.Name]?.ToString()?.Trim() ?? "غير محدد";
                var valObj = row[valCol.Name]?.ToString()?.Trim();

                if (double.TryParse(valObj, out double dVal))
                {
                    if (!groups.ContainsKey(catVal))
                        groups[catVal] = 0;
                    groups[catVal] += dVal;
                }
            }

            var topGroups = groups.OrderByDescending(g => g.Value).Take(12).ToList();
            categories = topGroups.Select(g => g.Key).ToList();
            values = topGroups.Select(g => Math.Round(g.Value, 1)).ToList();
        }
        else if (valCol != null)
        {
            categories.Add("المتوسط");
            values.Add(valCol.MeanValue ?? 0);
            categories.Add("الوسيط");
            values.Add(valCol.MedianValue ?? 0);
            categories.Add("الانحراف المعياري");
            values.Add(valCol.StandardDeviation ?? 0);
            categories.Add("الأدنى");
            values.Add(valCol.MinValue ?? 0);
            categories.Add("الأعلى");
            values.Add(valCol.MaxValue ?? 0);
        }

        var categoriesJson = JsonSerializer.Serialize(categories);
        var valuesJson = JsonSerializer.Serialize(values);
        var title = catCol != null && valCol != null ? $"{valCol.Name} حسب {catCol.Name}" : "المؤشرات الإحصائية الرئيسية";

        var bgColor = isDark ? "#131B2A" : "#FFFFFF";
        var textColor = isDark ? "#E2E8F0" : "#1E293B";
        var gridColor = isDark ? "#1E293B" : "#E2E8F0";

        return $@"<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <script src=""https://cdn.jsdelivr.net/npm/apexcharts""></script>
    <style>
        body {{
            margin: 0;
            padding: 12px;
            background-color: {bgColor};
            color: {textColor};
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            overflow: hidden;
        }}
        #chart {{
            width: 100%;
            height: 100%;
        }}
    </style>
</head>
<body>
    <div id=""chart""></div>
    <script>
        const isDonut = '{chartType}' === 'donut' || '{chartType}' === 'pie';
        const options = {{
            chart: {{
                type: '{chartType}',
                height: '100%',
                background: 'transparent',
                toolbar: {{ show: true }},
                fontFamily: 'Segoe UI, Tahoma'
            }},
            theme: {{
                mode: '{(isDark ? "dark" : "light")}'
            }},
            colors: ['#0EA5E9', '#6366F1', '#10B981', '#F59E0B', '#F43F5E', '#8B5CF6', '#14B8A6'],
            series: isDonut 
                ? {valuesJson}
                : [{{ name: '{valCol?.Name ?? "القيمة"}', data: {valuesJson} }}],
            labels: isDonut ? {categoriesJson} : undefined,
            xaxis: isDonut ? undefined : {{
                categories: {categoriesJson},
                labels: {{ style: {{ colors: '{textColor}' }} }}
            }},
            yaxis: isDonut ? undefined : {{
                labels: {{ style: {{ colors: '{textColor}' }} }}
            }},
            grid: {{
                borderColor: '{gridColor}'
            }},
            title: {{
                text: '{title}',
                align: 'right',
                style: {{ fontSize: '15px', fontWeight: 'bold', color: '{textColor}' }}
            }},
            dataLabels: {{
                enabled: isDonut
            }},
            stroke: {{
                curve: 'smooth',
                width: '{chartType}' === 'line' || '{chartType}' === 'area' ? 3 : 1
            }}
        }};

        const chart = new ApexCharts(document.querySelector(""#chart""), options);
        chart.render();
    </script>
</body>
</html>";
    }
}
