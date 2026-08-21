using R3;

namespace UniScan.Client.App.Pipeline;

public interface ITaskContext
{
    public BindableReactiveProperty<string> Status { get; }//todo why not give setter func to top level reactiveproperty???
}