using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataVision.Desktop.Core.Interfaces;

namespace DataVision.Desktop.ViewModels;

public class ConsoleLogEntry
{
    public string Sender { get; set; } = "User"; // "User" or "System"
    public string Message { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Now;
    public bool IsSystem => Sender == "System";
    public string TimeFormatted => Time.ToString("HH:mm:ss");
}

/// <summary>
/// نموذج العرض لوحدة الاستعلام الإحصائي والفحص التنفيذي التفاعلي.
/// Follows MVVM & SOLID.
/// </summary>
public partial class AnalyticsSummaryViewModel : ObservableObject
{
    private readonly IAnalyticsEngineService _analyticsEngine;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isComputing;

    public ObservableCollection<ConsoleLogEntry> Logs { get; } = new();

    public AnalyticsSummaryViewModel(IAnalyticsEngineService analyticsEngine, MainViewModel mainViewModel)
    {
        _analyticsEngine = analyticsEngine;
        _mainViewModel = mainViewModel;

        Logs.Add(new ConsoleLogEntry
        {
            Sender = "System",
            Message = "مرحباً بك في وحدة التحليل الإحصائي والاستعلام التنفيذي. يمكنك الاستفسار عن: (الأعمدة، المؤشرات الإحصائية، القيم الشاذة، أو ترتيب الفئات) وسيقوم المحرك بحساب النتائج فوراً دون أي اتصال خارجي."
        });
    }

    [RelayCommand]
    public async Task ExecuteQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsComputing) return;

        var query = UserInput.Trim();
        UserInput = string.Empty;

        Logs.Add(new ConsoleLogEntry { Sender = "User", Message = query });

        if (_mainViewModel.CurrentProfile == null)
        {
            Logs.Add(new ConsoleLogEntry
            {
                Sender = "System",
                Message = "تنبيه: يرجى فتح ملف إكسل أو CSV أولاً من لوحة التحكم لتنفيذ الاستعلامات الإحصائية عليه."
            });
            return;
        }

        try
        {
            IsComputing = true;
            var result = await _analyticsEngine.ExecuteStatisticalQueryAsync(_mainViewModel.CurrentProfile, query);
            Logs.Add(new ConsoleLogEntry { Sender = "System", Message = result });
        }
        catch (Exception ex)
        {
            Logs.Add(new ConsoleLogEntry
            {
                Sender = "System",
                Message = $"خطأ أثناء تنفيذ الاستعلام: {ex.Message}"
            });
        }
        finally
        {
            IsComputing = false;
        }
    }
}
