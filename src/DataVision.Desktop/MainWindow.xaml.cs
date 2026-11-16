using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using DataVision.Desktop.ViewModels;
using DataVision.Desktop.Views;
using WinRT.Interop;

namespace DataVision.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private AppWindow? _appWindow;

    public MainWindow()
    {
        App.MainWindow = this;
        InitializeComponent();
        _mainViewModel = App.Services.GetRequiredService<MainViewModel>();

        SetupTitleBar();
        SetupStatusBinding();

        // Default to Dashboard
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void SetupTitleBar()
    {
        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (_appWindow != null && AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.White;
            }
        }
        catch { }
    }

    private void SetupStatusBinding()
    {
        _mainViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StatusMessage))
            {
                TxtStatus.Text = _mainViewModel.StatusMessage;
            }
            else if (e.PropertyName == nameof(MainViewModel.IsBusy))
            {
                StatusRing.IsActive = _mainViewModel.IsBusy;
                StatusRing.Visibility = _mainViewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            }
        };
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            switch (tag)
            {
                case "dashboard":
                    ContentFrame.Navigate(typeof(DashboardPage));
                    break;
                case "charts":
                    ContentFrame.Navigate(typeof(ChartsPage));
                    break;
                case "analytics":
                    ContentFrame.Navigate(typeof(AnalyticsSummaryPage));
                    break;
                case "preview":
                    ContentFrame.Navigate(typeof(DataPreviewPage));
                    break;
            }
        }
    }
}
