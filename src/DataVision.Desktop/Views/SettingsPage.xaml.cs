using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.Services;

namespace DataVision.Desktop.Views;

public sealed partial class SettingsPage : Page
{
    private readonly AiAnalysisService _aiService;

    public SettingsPage()
    {
        InitializeComponent();
        _aiService = App.Services.GetRequiredService<AiAnalysisService>();

        if (!string.IsNullOrEmpty(_aiService.GeminiApiKey))
        {
            TxtApiKey.Password = _aiService.GeminiApiKey;
        }
    }

    private void TxtApiKey_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _aiService.GeminiApiKey = TxtApiKey.Password.Trim();
    }

    private void BtnSaveKey_Click(object sender, RoutedEventArgs e)
    {
        _aiService.GeminiApiKey = TxtApiKey.Password.Trim();
    }
}
