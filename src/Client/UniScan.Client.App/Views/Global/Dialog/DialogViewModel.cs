using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Global.Dialog;

public partial class DialogViewModel : ViewModelBase
{
    public static readonly ICommand DefaultConfirmCommand = new RelayCommand(() =>
    {
        DialogHost.Close("MainDialogHost");
    }, () => true);
    
    public ITextComponent TitleText { get; set; } =
        new TranslatableTextComponent(Identifier.CreateInstance("UniScan:client/i18n/view/dialog/default_title"));
    public ITextComponent ContentText { get; set; } = new TranslatableTextComponent(Identifier.CreateInstance("UniScan:client/i18n/view/dialog/default_content"));

    public ICommand ConfirmCommand { get; set; } = DefaultConfirmCommand;
}