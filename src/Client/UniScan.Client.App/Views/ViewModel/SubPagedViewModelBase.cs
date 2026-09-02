using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Shiki.Common.Identity;

namespace UniScan.Client.App.Views.ViewModel;

public record NavigationMessage(ObservableObject Subpage);

public interface ISingletonSubPagedViewModel : ISubPagedViewModel
{
    new static abstract Identifier Identifier { get; }
}

public interface ISubPagedViewModel : ISubPagedViewNavigator
{
    ObservableObject CurrentSubpage { get; }
    Identifier Identifier { get; }
}

public abstract partial class SubPagedViewModelBase : ViewModelBase, ISubPagedViewModel
{
    private ISubPagedViewNavigator _subPagedViewNavigatorImplementation;

    [ObservableProperty]
    public partial ObservableObject CurrentSubpage { get; set; }

    public Identifier Identifier { get; }

    protected SubPagedViewModelBase(ObservableObject subpage, Identifier identifier)
    {
        CurrentSubpage = subpage;
        Identifier = identifier;
        
        WeakReferenceMessenger.Default.Register<NavigationMessage, Identifier>(this, Identifier, OnSwitchSubpageMessageReceived);
    }

    protected virtual void OnSwitchSubpageMessageReceived(object recipient, NavigationMessage message)
    {
        CurrentSubpage = message.Subpage;
    }


    void ISubPagedViewNavigator.Navigate(ObservableObject page)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CurrentSubpage = page;
        });
    }
}

public interface ISubPagedViewNavigator
{
    void Navigate(ObservableObject page);
}

public abstract partial class SingletonSubPagedViewModelBase<TSingletonSelf>(ObservableObject subpage)
    : SubPagedViewModelBase(subpage, TSingletonSelf.Identifier)
    where TSingletonSelf : class, ISingletonSubPagedViewModel;