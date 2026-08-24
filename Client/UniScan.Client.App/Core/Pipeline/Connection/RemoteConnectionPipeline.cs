using System;
using System.Threading.Tasks;
using R3;
using Serilog;

namespace UniScan.Client.App.Core.Pipeline.Connection;

public partial class RemoteConnectionPipeline
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
                  .ThenRun((_, _) =>
                   {
                       _subscription?.Dispose();

                       return Task.CompletedTask;
                   })
                  .Build();
        
        _subscription = Pipeline.Status.Subscribe(s => { _logger.Information("{Status}", s); });
    }
}