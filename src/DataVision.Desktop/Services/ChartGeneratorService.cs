using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.Services;

/// <summary>
/// خدمة توليد المخططات والرسوم البيانية التفاعلية ولوحات التحكم التنفيذية الشبيهة بـ Power BI.
/// Implements IChartGeneratorService following SOLID.
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
            : profile.Columns.FirstOrDefault(c => !c.IsNumeric && c.UniqueCount is >= 2 and <= 35);

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
                var catVal = row[catCol.Name]?.ToString()?.Trim() ?? "أخرى";
                var valObj = row[valCol.Name]?.ToString()?.Trim();

                if (double.TryParse(valObj, out double dVal))
                {
                    if (!groups.ContainsKey(catVal))
                        groups[catVal] = 0;
                    groups[catVal] += dVal;
                }
            }

            var topGroups = groups.OrderByDescending(g => g.Value).Take(10).ToList();
            categories = topGroups.Select(g => g.Key).ToList();
            values = topGroups.Select(g => Math.Round(g.Value, 1)).ToList();
        }
        else if (valCol != null)
        {
            categories.Add("المتوسط");
            values.Add(valCol.MeanValue ?? 0);
            categories.Add("الوسيط");
            values.Add(valCol.MedianValue ?? 0);
            categories.Add("الانحراف");
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

        // Multi-Visual Power BI Dashboard Layout
        return $@"<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <script src=""https://cdn.jsdelivr.net/npm/apexcharts""></script>
    <style>
        body {{
            margin: 0;
            padding: 10px;
            background-color: {bgColor};
            color: {textColor};
            font-family: 'Segoe UI', Tahoma, sans-serif;
            overflow-x: hidden;
            box-sizing: border-box;
        }}
        .charts-container {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 12px;
            width: 100%;
            height: calc(100vh - 20px);
        }}
        .chart-box {{
            background: #0F172A;
            border: 1px solid #1E293B;
            border-radius: 10px;
            padding: 8px;
            height: 100%;
            box-sizing: border-box;
        }}
        @media (max-width: 650px) {{
            .charts-container {{ grid-template-columns: 1fr; }}
        }}
    </style>
</head>
<body>
    <div class=""charts-container"">
        <div class=""chart-box"">
            <div id=""chartBar""></div>
        </div>
        <div class=""chart-box"">
            <div id=""chartDonut""></div>
        </div>
    </div>

    <script>
        const categories = {categoriesJson};
        const values = {valuesJson};

        // 1. Primary Bar / Column Chart
        const barOptions = {{
            chart: {{
                type: 'bar',
                height: '92%',
                background: 'transparent',
                toolbar: {{ show: true }},
                fontFamily: 'Segoe UI, Tahoma'
            }},
            theme: {{ mode: '{(isDark ? "dark" : "light")}' }},
            colors: ['#0EA5E9', '#6366F1', '#10B981', '#F59E0B'],
            series: [{{ name: '{valCol?.Name ?? "القيمة"}', data: values }}],
            xaxis: {{
                categories: categories,
                labels: {{ style: {{ colors: '{textColor}', fontSize: '11px' }} }}
            }},
            yaxis: {{ labels: {{ style: {{ colors: '{textColor}' }} }} }},
            grid: {{ borderColor: '{gridColor}' }},
            title: {{
                text: '{title}',
                align: 'right',
                style: {{ fontSize: '13px', fontWeight: 'bold', color: '{textColor}' }}
            }},
            plotOptions: {{
                bar: {{ borderRadius: 4, columnWidth: '55%', distributed: true }}
            }}
        }};
        new ApexCharts(document.querySelector(""#chartBar""), barOptions).render();

        // 2. Secondary Donut / Distribution Chart
        const donutOptions = {{
            chart: {{
                type: 'donut',
                height: '92%',
                background: 'transparent',
                toolbar: {{ show: false }},
                fontFamily: 'Segoe UI, Tahoma'
            }},
            theme: {{ mode: '{(isDark ? "dark" : "light")}' }},
            colors: ['#0EA5E9', '#6366F1', '#10B981', '#F59E0B', '#F43F5E', '#8B5CF6', '#14B8A6'],
            series: values.length > 0 ? values : [1],
            labels: categories.length > 0 ? categories : ['البيانات'],
            title: {{
                text: 'توزيع الحصص والنسب المئوية',
                align: 'right',
                style: {{ fontSize: '13px', fontWeight: 'bold', color: '{textColor}' }}
            }},
            legend: {{ position: 'bottom', labels: {{ colors: '{textColor}' }} }},
            dataLabels: {{ enabled: true }}
        }};
        new ApexCharts(document.querySelector(""#chartDonut""), donutOptions).render();
    </script>
</body>
</html>";
    }
}
