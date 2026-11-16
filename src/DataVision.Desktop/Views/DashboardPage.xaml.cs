using System;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;
using DataVision.Desktop.Services;
using WinRT.Interop;

namespace DataVision.Desktop.Views;

public sealed partial class DashboardPage : Page
{
    private readonly MainViewModel? _mainViewModel;
    private readonly ChartGeneratorService? _chartGenerator;

    public DashboardPage()
    {
        _mainViewModel = App.Services.GetService<MainViewModel>();
        _chartGenerator = App.Services.GetService<ChartGeneratorService>();

        InitializeComponent();

        Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDashboard();
    }

    private async void BtnOpenFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var hwnd = IntPtr.Zero;
            if (App.MainWindow != null)
            {
                hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            }

            var selectedPath = NativeFileDialog.ShowOpenFileDialog(hwnd, "اختر ملف إكسل أو CSV للتحليل");
            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath) && _mainViewModel != null)
            {
                await _mainViewModel.LoadFileAsync(selectedPath);
                RefreshDashboard();
            }
        }
        catch (Exception ex)
        {
            if (_mainViewModel != null)
            {
                _mainViewModel.StatusMessage = $"خطأ أثناء فتح مربع اختيار الملف: {ex.Message}";
            }
        }
    }

    public async void RefreshDashboard()
    {
        if (_mainViewModel == null) return;

        var profile = _mainViewModel.CurrentProfile;
        if (profile == null)
        {
            TxtTotalRows.Text = "0";
            TxtTotalColumns.Text = "0";
            TxtDataQuality.Text = "100%";
            TxtActiveSheet.Text = "لم يتم تحديد ملف";
            TxtFileSize.Text = "0 KB";

            await RenderPlaceholderChart();
            return;
        }

        TxtTotalRows.Text = profile.TotalRows.ToString("N0");
        TxtTotalColumns.Text = profile.TotalColumns.ToString();
        TxtActiveSheet.Text = profile.SheetName;
        TxtFileSize.Text = profile.FileSizeFormatted;

        int totalCells = profile.TotalRows * profile.TotalColumns;
        int totalMissing = profile.Columns.Sum(c => c.NullCount);
        double health = totalCells > 0 ? Math.Round((1.0 - (double)totalMissing / totalCells) * 100.0, 1) : 100;
        TxtDataQuality.Text = $"{health}%";

        LvInsights.ItemsSource = _mainViewModel.Insights;

        // Render live chart
        try
        {
            if (ChartWebView != null && _chartGenerator != null)
            {
                await ChartWebView.EnsureCoreWebView2Async();
                var html = _chartGenerator.GenerateApexChartHtml(profile, chartType: "bar", isDark: true);
                ChartWebView.NavigateToString(html);
            }
        }
        catch { }
    }

    private async System.Threading.Tasks.Task RenderPlaceholderChart()
    {
        try
        {
            if (ChartWebView != null)
            {
                await ChartWebView.EnsureCoreWebView2Async();
                var html = @"<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head><meta charset=""UTF-8""><style>body{margin:0;display:flex;align-items:center;justify-content:center;height:100vh;background:#131B2A;color:#64748B;font-family:sans-serif;}</style></head>
<body><h3>قم برفع ملف إكسل أو CSV لعرض المخطط البياني التفاعلي هنا</h3></body></html>";
                ChartWebView.NavigateToString(html);
            }
        }
        catch { }
    }
}
