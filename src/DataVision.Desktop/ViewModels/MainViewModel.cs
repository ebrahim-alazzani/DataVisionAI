using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVision.Desktop.Models;
using DataVision.Desktop.Services;

namespace DataVision.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ExcelReaderService _excelReader;
    private readonly AiAnalysisService _aiService;

    [ObservableProperty]
    private string _appTitle = "DataVision AI · منصة تحليل البيانات الذكية";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "جاهز لاستقبال ملف الإكسل أو CSV";

    [ObservableProperty]
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private DataTableProfile? _currentProfile;

    [ObservableProperty]
    private string _currentPage = "dashboard";

    [ObservableProperty]
    private ObservableCollection<AiInsight> _insights = new();

    public MainViewModel(ExcelReaderService excelReader, AiAnalysisService aiService)
    {
        _excelReader = excelReader;
        _aiService = aiService;
    }

    [RelayCommand]
    public async Task LoadFileAsync(string filePath)
    {
        try
        {
            IsBusy = true;
            StatusMessage = "جاري قراءة واستخراج بيانات الجدول...";

            CurrentProfile = await _excelReader.LoadAndProfileAsync(filePath);

            StatusMessage = "جاري تحليل البيانات بواسطة محرك الذكاء الاصطناعي...";
            var generatedInsights = await _aiService.GenerateInsightsAsync(CurrentProfile);

            Insights.Clear();
            foreach (var insight in generatedInsights)
            {
                Insights.Add(insight);
            }

            StatusMessage = $"تم تحميل ({CurrentProfile.FileName}) بنجاح: {CurrentProfile.TotalRows:N0} صفاً و {CurrentProfile.TotalColumns} أعمدة.";
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

    [RelayCommand]
    public void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }
}
