using R3;

namespace UniScan.Client.App.Pipeline;

public interface ITaskContext
{
    public BindableReactiveProperty<string> Status { get; }//todo why not give setter func to top level reactiveproperty???
}

public interface ITaskContext<out TSelf, in TOldContext> : ITaskContext
    where TSelf : ITaskContext<TSelf, TOldContext>
    where TOldContext : ITaskContext
{
    static abstract TSelf TransitionFrom(TOldContext oldContext);
}