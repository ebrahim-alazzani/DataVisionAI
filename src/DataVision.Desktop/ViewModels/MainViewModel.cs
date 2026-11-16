using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVision.Desktop.Core.Enums;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.ViewModels;

/// <summary>
/// نموذج العرض الرئيسي (MainViewModel) الذي ينسق دورة حياة التطبيق والمحركات الثلاثة.
/// Coordinates the 3 Analytical Engines:
/// 1. Native C# Statistical & Algorithmic Engine (100% Offline & Instant)
/// 2. Python Data Science Engine (Pandas + NumPy)
/// 3. Machine Learning Predictive Modeling Engine (Scikit-Learn Regression & Forecasting)
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IExcelReaderService _excelReader;
    private readonly IAnalyticsEngineService _analyticsEngine;
    private readonly IPythonAnalyticsService _pythonAnalytics;
    private readonly IReportExportService _reportExport;

    [ObservableProperty]
    private string _appTitle = "DataVision Studio · منصة استخبارات الأعمال والتحليلات الإحصائية المتقدمة";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "جاهز لاستقبال ملف الإكسل أو CSV";

    [ObservableProperty]
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private DataTableProfile? _currentProfile;

    [ObservableProperty]
    private ObservableCollection<ExecutiveInsight> _insights = new();

    public MainViewModel(
        IExcelReaderService excelReader,
        IAnalyticsEngineService analyticsEngine,
        IPythonAnalyticsService pythonAnalytics,
        IReportExportService reportExport)
    {
        _excelReader = excelReader;
        _analyticsEngine = analyticsEngine;
        _pythonAnalytics = pythonAnalytics;
        _reportExport = reportExport;
    }

    [RelayCommand]
    public async Task LoadFileAsync(string filePath)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "1/3 جاري قراءة واستخراج بيانات الجدول وتوصيف الأعمدة (محرك C# الأصلي)...";

            CurrentProfile = await _excelReader.LoadAndProfileAsync(filePath);

            StatusMessage = "2/3 جاري فحص الانحرافات واكتشاف القيم الشاذة إحصائياً (Tukey IQR & Pareto)...";
            var generatedInsights = await _analyticsEngine.GenerateExecutiveInsightsAsync(CurrentProfile);

            Insights.Clear();
            foreach (var insight in generatedInsights)
            {
                Insights.Add(insight);
            }

            // 3. Invoke Python Scientific & Machine Learning Engine
            StatusMessage = "3/3 جاري استدعاء محرك بايثون العلمي ونموذج التعلم الآلي التنبؤي (Pandas & Scikit-Learn)...";
            var pyResult = await _pythonAnalytics.AnalyzeAsync(filePath);
            if (pyResult != null && pyResult.Success)
            {
                // Add Top Correlation Insight
                if (pyResult.Correlations.Count > 0)
                {
                    var topCorr = pyResult.Correlations[0];
                    Insights.Insert(0, new ExecutiveInsight
                    {
                        Title = $"ارتباط إحصائي متبادل: ({topCorr.Col1}) و ({topCorr.Col2})",
                        Description = $"أظهر محرك بايثون معامل ارتباط بيرسون مقداره {topCorr.Correlation:N3} بدرجة قوة ({topCorr.Strength}). يشير ذلك إلى علاقة تأثير مباشرة بين هذين المتغيرين.",
                        Category = InsightCategory.Trend,
                        Severity = Math.Abs(topCorr.Correlation) > 0.6 ? InsightSeverity.Success : InsightSeverity.Info,
                        ImpactMetric = $"r = {topCorr.Correlation:N2}"
                    });
                }

                // Add Machine Learning Predictive Forecast Insight
                if (pyResult.MlForecast != null)
                {
                    var ml = pyResult.MlForecast;
                    Insights.Insert(0, new ExecutiveInsight
                    {
                        Title = $"نموذج التعلم الآلي التنبؤي لـ ({ml.TargetColumn})",
                        Description = $"تم تدريب نموذج انحدار خطي (Linear Regression) بمعامل تحديد R² = {ml.R2Accuracy}. الاتجاه العام {ml.TrendDirection}. التنبؤات المتوقعة للفترات التالية: [{string.Join(", ", ml.ForecastNextSteps)}].",
                        Category = InsightCategory.Recommendation,
                        Severity = InsightSeverity.Success,
                        ImpactMetric = ml.TrendDirection.Contains("صاعد") ? "اتجاه صاعد" : "اتجاه هابط"
                    });
                }

                // Add Clustering Insight
                if (pyResult.Clusters.Count > 0)
                {
                    Insights.Add(new ExecutiveInsight
                    {
                        Title = $"تصنيف الأنماط العنقودية (K-Means Clustering: {pyResult.Clusters.Count} شرائح)",
                        Description = $"قام نموذج التعلم الآلي غير الخاضع للإشراف بتوزيع السجلات إلى {pyResult.Clusters.Count} مجموعات متجانسة لتحديد الأنماط السلوكية والتشغيلية.",
                        Category = InsightCategory.Distribution,
                        Severity = InsightSeverity.Info,
                        ImpactMetric = $"{pyResult.Clusters.Count} مجموعات"
                    });
                }
            }

            StatusMessage = $"تم تحليل ({CurrentProfile.FileName}) بنجاح: {CurrentProfile.TotalRows:N0} سجلاً و {CurrentProfile.TotalColumns} أعمدة بجودة {CurrentProfile.OverallQualityScore}%.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"خطأ أثناء معالجة الملف: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<string?> ExportReportAsync(string format = "html")
    {
        if (CurrentProfile == null) return null;

        try
        {
            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var baseName = Path.GetFileNameWithoutExtension(CurrentProfile.FileName);
            var ext = format.ToLower() == "markdown" ? "md" : "html";
            var outPath = Path.Combine(desktopDir, $"{baseName}_Executive_Report_{DateTime.Now:yyyyMMdd_HHmm}.{ext}");

            if (ext == "md")
            {
                await _reportExport.ExportMarkdownReportAsync(CurrentProfile, Insights, outPath);
            }
            else
            {
                await _reportExport.ExportHtmlReportAsync(CurrentProfile, Insights, outPath);
            }

            StatusMessage = $"تم تصدير التقرير التنفيذي بنجاح إلى: {Path.GetFileName(outPath)}";
            return outPath;
        }
        catch (Exception ex)
        {
            StatusMessage = $"فشل تصدير التقرير: {ex.Message}";
            return null;
        }
    }
}
