using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Shiki.Common.Serialization.Registry;
using Shiki.Common.Serialization.Registry.Validators;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.UI;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
                             "Default implementation of ViewLocator involves reflection which may be trimmed away.",
                             Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    //TODO in the future don't we want to use some registry for this or similar for AOT compatibility?
    public static ViewLocator Instance { get; } = new();
    
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
    
        string name = param.GetType().FullName!.Replace("ControlViewModel", "Control", StringComparison.Ordinal).Replace("ViewModel", "View", StringComparison.Ordinal);
        Type? type = GlobalTypeRegistry<IHasNameTypeRegistryValidator<Control>>.GetScope(typeof(Control)).FirstOrDefault(kvp => kvp.Key == name).Value;
    
        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
    
        return new TextBlock { Text = "Not Found: " + name };
    }
    
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}