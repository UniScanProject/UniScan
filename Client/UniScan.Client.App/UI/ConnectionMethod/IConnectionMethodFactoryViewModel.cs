using UniScan.Client.App.ViewModels;
using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Client.App.UI.ConnectionMethod;

public interface IConnectionMethodFactoryViewModel
{
    public IRemoteConnectionMethod Create();
    
    public bool IsValid { get; }
}

public interface IConnectionMethodFactoryViewModel<TSelf, out TConnectionMethod> : IConnectionMethodFactoryViewModel
where TSelf : ViewModelBase, IConnectionMethodFactoryViewModel<TSelf, TConnectionMethod>
where TConnectionMethod : IRemoteConnectionMethod
{
    public new TConnectionMethod Create();

    IRemoteConnectionMethod IConnectionMethodFactoryViewModel.Create() => Create();
}