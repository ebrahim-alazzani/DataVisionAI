using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataVision.Desktop.Core.Interfaces;

namespace DataVision.Desktop.Services;

/// <summary>
/// خدمة استدعاء محرك بايثون العلمي ونماذج التعلم الآلي (Pandas + Scikit-Learn).
/// Implements IPythonAnalyticsService following SOLID.
/// </summary>
public class PythonAnalyticsService : IPythonAnalyticsService
{
    private string _pythonPath = "python";
    private bool _isPythonAvailable = false;

    public bool IsPythonAvailable => _isPythonAvailable;
    public string PythonPath => _pythonPath;

    public PythonAnalyticsService()
    {
        DetectPython();
    }

    private void DetectPython()
    {
        // Check neighboring virtual environment in E:\New folder (3)\ai_model_studio\.venv
        string[] candidates =
        {
            @"E:\New folder (3)\ai_model_studio\.venv\Scripts\python.exe",
            @"E:\New folder (3)\CV_Waste_Sorting_Colab\.venv\Scripts\python.exe",
            @"E:\New folder (3)\nabd_ai\.venv\Scripts\python.exe",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".venv", "Scripts", "python.exe"),
            "python.exe",
            "python"
        };

        foreach (var c in candidates)
        {
            if (File.Exists(c))
            {
                _pythonPath = c;
                _isPythonAvailable = true;
                return;
            }
        }

        _pythonPath = "python";
        _isPythonAvailable = true;
    }

    public async Task<PythonAnalyticsResult?> AnalyzeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "python_analytics_engine.py");
                if (!File.Exists(scriptPath))
                {
                    // Fallback relative to project source
                    scriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Scripts\python_analytics_engine.py"));
                }

                if (!File.Exists(scriptPath))
                {
                    return new PythonAnalyticsResult { Success = false, Error = "ملف سكريبت بايثون غير موجود." };
                }

                var psi = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"\"{scriptPath}\" --input \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using var proc = Process.Start(psi);
                if (proc == null) return null;

                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                proc.WaitForExit(15000);

                if (string.IsNullOrWhiteSpace(output))
                {
                    return new PythonAnalyticsResult { Success = false, Error = error };
                }

                using var doc = JsonDocument.Parse(output);
                var root = doc.RootElement;

                var res = new PythonAnalyticsResult
                {
                    Success = root.TryGetProperty("success", out var s) && s.GetBoolean(),
                    Engine = root.TryGetProperty("engine", out var eng) ? eng.GetString() ?? "" : "",
                    Rows = root.TryGetProperty("rows", out var r) ? r.GetInt32() : 0,
                    Columns = root.TryGetProperty("columns", out var c) ? c.GetInt32() : 0
                };

                if (root.TryGetProperty("correlations", out var corrs) && corrs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in corrs.EnumerateArray())
                    {
                        res.Correlations.Add(new CorrelationPair
                        {
                            Col1 = item.GetProperty("col1").GetString() ?? "",
                            Col2 = item.GetProperty("col2").GetString() ?? "",
                            Correlation = item.GetProperty("correlation").GetDouble(),
                            Strength = item.GetProperty("strength").GetString() ?? ""
                        });
                    }
                }

                if (root.TryGetProperty("ml_forecast", out var ml) && ml.ValueKind == JsonValueKind.Object)
                {
                    var forecast = new MlForecastResult
                    {
                        TargetColumn = ml.GetProperty("target_column").GetString() ?? "",
                        ModelType = ml.GetProperty("model_type").GetString() ?? "",
                        TrendDirection = ml.GetProperty("trend_direction").GetString() ?? "",
                        SlopePerStep = ml.GetProperty("slope_per_step").GetDouble(),
                        R2Accuracy = ml.GetProperty("r2_accuracy").GetDouble()
                    };

                    if (ml.TryGetProperty("forecast_next_steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var step in steps.EnumerateArray())
                        {
                            forecast.ForecastNextSteps.Add(step.GetDouble());
                        }
                    }

                    res.MlForecast = forecast;
                }

                if (root.TryGetProperty("insights", out var inArray) && inArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var i in inArray.EnumerateArray())
                    {
                        var title = i.GetProperty("title").GetString() ?? "";
                        var desc = i.GetProperty("description").GetString() ?? "";
                        res.InsightsSummary.Add($"[{title}]: {desc}");
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                return new PythonAnalyticsResult { Success = false, Error = ex.Message };
            }
        }, cancellationToken);
    }
}
