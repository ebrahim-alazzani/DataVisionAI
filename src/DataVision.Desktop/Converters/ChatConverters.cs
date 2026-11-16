using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DataVision.Desktop.Converters;

public class BoolToAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isAi && isAi)
            return HorizontalAlignment.Left;
        return HorizontalAlignment.Right;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BoolToBgBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush AiBrush = new(ColorHelper.FromArgb(255, 15, 23, 42));      // #0F172A
    private static readonly SolidColorBrush UserBrush = new(ColorHelper.FromArgb(255, 30, 41, 59));     // #1E293B

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isAi && isAi)
            return AiBrush;
        return UserBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class BoolToSenderColorConverter : IValueConverter
{
    private static readonly SolidColorBrush AiColor = new(ColorHelper.FromArgb(255, 56, 189, 248));     // #38BDF8
    private static readonly SolidColorBrush UserColor = new(ColorHelper.FromArgb(255, 129, 140, 248));   // #818CF8

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isAi && isAi)
            return AiColor;
        return UserColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
