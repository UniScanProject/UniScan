using R3;
using Serilog;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Builder;

namespace UniScan.Client.Core.Remote.Pipeline;

public partial class RemoteConnectionPipeline : IDisposable
{
    public TaskPipelineBuilder<TaskContexts.ConnectionContext, TaskContexts.RemoteContext> PipelineBuilder { get; }
    private readonly ILogger _logger = Log.ForContext<RemoteConnectionPipeline>();
    private readonly IDisposable _subscription;

    public RemoteConnectionPipeline()
    {
        PipelineBuilder = new TaskPipelineBuilder<TaskContexts.ConnectionContext, TaskContexts.ConnectionContext>()
                         .ThenRunAsync(StartConnection)
                         .ThenTransitionTo<TaskContexts.NegotiationContext>()
                         .ThenRunAsync(Handshake)
                         .ThenTransitionTo<TaskContexts.RemoteContext>()
                         .ThenRunAsync(GetDeviceList);
        
        _subscription = PipelineBuilder.ExecutionState.CurrentStatus.AsObservable().Subscribe(s => { _logger.Information("{Status}", s); });
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}