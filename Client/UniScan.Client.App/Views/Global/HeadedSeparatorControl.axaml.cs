using Avalonia;
using Avalonia.Controls;

namespace UniScan.Client.App.Views.Global;

public partial class HeadedSeparatorControl : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<HeadedSeparatorControl, string>(nameof(Text), defaultValue: "");

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public HeadedSeparatorControl()
    {
        InitializeComponent();
    }
}