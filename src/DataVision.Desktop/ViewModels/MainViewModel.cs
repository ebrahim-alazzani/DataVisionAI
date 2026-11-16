using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVision.Desktop.Core.Interfaces;
using DataVision.Desktop.Models;

namespace DataVision.Desktop.ViewModels;

/// <summary>
/// نموذج العرض الرئيسي (MainViewModel) الذي ينسق دورة حياة التطبيق والبيانات.
/// Follows MVVM & SOLID: Single Responsibility Principle & Dependency Inversion Principle.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IExcelReaderService _excelReader;
    private readonly IAnalyticsEngineService _analyticsEngine;
    private readonly IReportExportService _reportExport;

    [ObservableProperty]
    private string _appTitle = "DataVision Studio · منصة استخبارات الأعمال والتحليلات المتقدمة";

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
        IReportExportService reportExport)
    {
        _excelReader = excelReader;
        _analyticsEngine = analyticsEngine;
        _reportExport = reportExport;
    }

    [RelayCommand]
    public async Task LoadFileAsync(string filePath)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "جاري قراءة واستخراج بيانات الجدول وفحص الأعمدة...";

            CurrentProfile = await _excelReader.LoadAndProfileAsync(filePath);

            StatusMessage = "جاري تنفيذ التحليل الإحصائي وتوليد المؤشرات التنفيذية...";
            var generatedInsights = await _analyticsEngine.GenerateExecutiveInsightsAsync(CurrentProfile);

            Insights.Clear();
            foreach (var insight in generatedInsights)
            {
                Insights.Add(insight);
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
