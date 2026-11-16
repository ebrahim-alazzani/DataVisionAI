using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;

namespace DataVision.Desktop.Views;

public sealed partial class AnalyticsSummaryPage : Page
{
    private readonly AnalyticsSummaryViewModel _viewModel;

    public AnalyticsSummaryPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<AnalyticsSummaryViewModel>();
        DataContext = _viewModel;
        LvConsole.ItemsSource = _viewModel.Logs;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AnalyticsSummaryViewModel.IsComputing))
            {
                PbComputing.Visibility = _viewModel.IsComputing ? Visibility.Visible : Visibility.Collapsed;
            }
        };
    }

    private async void BtnRunQuery_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteQuery();
    }

    private async void TxtInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await ExecuteQuery();
        }
    }

    private async void OnPillClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string text)
        {
            TxtInput.Text = text;
            await ExecuteQuery();
        }
    }

    private async System.Threading.Tasks.Task ExecuteQuery()
    {
        if (string.IsNullOrWhiteSpace(TxtInput.Text)) return;
        _viewModel.UserInput = TxtInput.Text;
        TxtInput.Text = string.Empty;
        await _viewModel.ExecuteQueryAsync();

        if (LvConsole.Items.Count > 0)
        {
            LvConsole.ScrollIntoView(LvConsole.Items[^1]);
        }
    }
}
