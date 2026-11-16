using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;

namespace DataVision.Desktop.Views;

public sealed partial class AiAnalystPage : Page
{
    private readonly AiAnalystViewModel _viewModel;

    public AiAnalystPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<AiAnalystViewModel>();
        DataContext = _viewModel;
        LvChat.ItemsSource = _viewModel.Messages;

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AiAnalystViewModel.IsThinking))
            {
                PbThinking.Visibility = _viewModel.IsThinking ? Visibility.Visible : Visibility.Collapsed;
            }
        };
    }

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
    {
        await SendCurrentMessage();
    }

    private async void TxtInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await SendCurrentMessage();
        }
    }

    private async void OnPillClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string text)
        {
            TxtInput.Text = text;
            await SendCurrentMessage();
        }
    }

    private async System.Threading.Tasks.Task SendCurrentMessage()
    {
        if (string.IsNullOrWhiteSpace(TxtInput.Text)) return;
        _viewModel.UserInput = TxtInput.Text;
        TxtInput.Text = string.Empty;
        await _viewModel.SendQuestionAsync();
        
        // Scroll to bottom
        if (LvChat.Items.Count > 0)
        {
            LvChat.ScrollIntoView(LvChat.Items[^1]);
        }
    }
}
