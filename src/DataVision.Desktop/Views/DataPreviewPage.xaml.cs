using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;

namespace DataVision.Desktop.Views;

public sealed partial class DataPreviewPage : Page
{
    private readonly MainViewModel _mainViewModel;

    public DataPreviewPage()
    {
        InitializeComponent();
        _mainViewModel = App.Services.GetRequiredService<MainViewModel>();
        Loaded += DataPreviewPage_Loaded;
    }

    private void DataPreviewPage_Loaded(object sender, RoutedEventArgs e)
    {
        var profile = _mainViewModel.CurrentProfile;
        if (profile != null)
        {
            LvColumns.ItemsSource = profile.Columns;
            TxtInfoBadge.Text = $"{profile.TotalColumns} أعمدة · {profile.TotalRows:N0} صفاً ({profile.SheetName})";
        }
        else
        {
            TxtInfoBadge.Text = "لم يتم فتح أي ملف بعد";
        }
    }
}
