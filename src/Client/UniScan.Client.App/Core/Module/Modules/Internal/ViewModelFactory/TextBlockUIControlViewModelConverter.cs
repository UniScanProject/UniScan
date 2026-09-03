using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using UniScan.Client.App.Core.Extensions;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.UserInterface;
using UniScan.UserInterface.Definitions;

namespace UniScan.Client.App.Core.Module.Modules.Internal.ViewModelFactory;

public class TextBlockUIControlViewModelConverter : IUiNodeViewModelConverter<TextBlockUIControl>
{
    public bool CanCreateView(IUINode node) => node is TextBlockUIControl;
    
    public object CreateView(TextBlockUIControl node)
    {
        Border b = node.Style.BuildStyledBorder();
        b.Child = new TextBlock
        {
            Text = node.Text,
            FontSize = node.FontSize
        };
        
        return b;
    }
}