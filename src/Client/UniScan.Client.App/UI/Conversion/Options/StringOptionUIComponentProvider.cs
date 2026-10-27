using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Shiki.Settings;
using Shiki.Settings.Option;

namespace UniScan.Client.App.UI.Conversion.Options;

public class StringOptionUIComponentProvider : IOptionUIComponentProvider
{
    public int Priority => 0;
    public bool SideMount => false;
    
    public bool CanProvide(IReadOnlyOption args) => args.Type == typeof(string);

    public Control Provide(IReadOnlyOption args)
    {
        TextBox box = new()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            PlaceholderText = "Enter value..."
        };

        box.Bind(TextBox.TextProperty, new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        });

        return box;
    }

}

public class BooleanOptionUIComponentProvider : IOptionUIComponentProvider
{
    public int Priority => 0;
    public bool SideMount => true;
    
    public bool CanProvide(IReadOnlyOption args) => args.Type == typeof(bool);

    public Control Provide(IReadOnlyOption args)
    {
        ToggleSwitch box = new()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        box.Bind(ToggleSwitch.IsCheckedProperty, new Binding("Value")
        {
            Mode = BindingMode.TwoWay
        });

        return box;
    }

}