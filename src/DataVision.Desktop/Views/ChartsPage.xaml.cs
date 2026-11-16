using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;
using DataVision.Desktop.Core.Interfaces;

namespace DataVision.Desktop.Views;

public sealed partial class ChartsPage : Page
{
    private readonly MainViewModel? _mainViewModel;
    private readonly IChartGeneratorService? _chartGenerator;
    private bool _isInitialized = false;

    public ChartsPage()
    {
        _mainViewModel = App.Services.GetService<MainViewModel>();
        _chartGenerator = App.Services.GetService<IChartGeneratorService>();

        InitializeComponent();
        _isInitialized = true;

        Loaded += ChartsPage_Loaded;
    }

    private void ChartsPage_Loaded(object sender, RoutedEventArgs e)
    {
        PopulateDropdowns();
        RenderChart();
    }

    private void PopulateDropdowns()
    {
        if (_mainViewModel?.CurrentProfile == null || CmbCategoryCol == null || CmbValueCol == null) return;

        var profile = _mainViewModel.CurrentProfile;
        CmbCategoryCol.Items.Clear();
        CmbValueCol.Items.Clear();

        foreach (var col in profile.Columns)
        {
            CmbCategoryCol.Items.Add(col.Name);
            if (col.IsNumeric)
            {
                CmbValueCol.Items.Add(col.Name);
            }
        }

        // Auto select best defaults
        var defaultCat = profile.Columns.FirstOrDefault(c => !c.IsNumeric && c.UniqueCount is >= 2 and <= 30);
        if (defaultCat != null)
            CmbCategoryCol.SelectedItem = defaultCat.Name;
        else if (CmbCategoryCol.Items.Count > 0)
            CmbCategoryCol.SelectedIndex = 0;

        var defaultVal = profile.Columns.FirstOrDefault(c => c.IsNumeric);
        if (defaultVal != null)
            CmbValueCol.SelectedItem = defaultVal.Name;
        else if (CmbValueCol.Items.Count > 0)
            CmbValueCol.SelectedIndex = 0;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        RenderChart();
    }

    private void BtnRefreshChart_Click(object sender, RoutedEventArgs e)
    {
        RenderChart();
    }

    private async void RenderChart()
    {
        if (!_isInitialized || _mainViewModel == null || _chartGenerator == null || MainChartView == null) return;

        var profile = _mainViewModel.CurrentProfile;
        if (profile == null)
        {
            try
            {
                await MainChartView.EnsureCoreWebView2Async();
                var html = @"<!DOCTYPE html><html lang=""ar"" dir=""rtl""><head><meta charset=""UTF-8""><style>body{margin:0;display:flex;align-items:center;justify-content:center;height:100vh;background:#131B2A;color:#64748B;font-family:sans-serif;}</style></head><body><h3>الرجاء فتح ملف إكسل من لوحة التحكم لتوليد المخططات</h3></body></html>";
                MainChartView.NavigateToString(html);
            }
            catch { }
            return;
        }

        var catCol = CmbCategoryCol?.SelectedItem?.ToString();
        var valCol = CmbValueCol?.SelectedItem?.ToString();

        var selectedItem = CmbChartType?.SelectedItem as ComboBoxItem;
        var chartType = selectedItem?.Tag?.ToString() ?? "bar";

        try
        {
            await MainChartView.EnsureCoreWebView2Async();
            var html = _chartGenerator.GenerateChartHtml(profile, catCol, valCol, chartType, isDark: true);
            MainChartView.NavigateToString(html);
        }
        catch { }
    }
}
