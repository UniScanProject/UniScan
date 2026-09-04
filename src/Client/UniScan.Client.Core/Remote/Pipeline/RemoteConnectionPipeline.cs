using R3;
using Serilog;
using Shiki.TaskPipeline;

namespace UniScan.Client.Core.Remote.Pipeline;

public partial class RemoteConnectionPipeline : IDisposable
{
    public TaskPipeline Pipeline { get; }
    private readonly ILogger _logger = Log.ForContext<RemoteConnectionPipeline>();
    private readonly IDisposable _subscription;

    public RemoteConnectionPipeline()
    {
        Pipeline = new TaskPipelineBuilder<TaskContexts.ConnectionContext>()
                  .ThenRun(StartConnection)
                  .ThenTransitionTo<TaskContexts.NegotiationContext>()
                  .ThenRun(Handshake)
                  .ThenTransitionTo<TaskContexts.RemoteContext>()
                  .ThenRun(GetDeviceList)
                  .Build();
        
        _subscription = Pipeline.Status.Subscribe(s => { _logger.Information("{Status}", s); });
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}