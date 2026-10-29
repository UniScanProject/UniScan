using R3;
using Serilog;
using Shiki.TaskPipeline.Builder;
using UniScan.Server.Core;

namespace UniScan.Server.Host.Pipeline;

public partial class UniScanServerHostInitializationPipeline
{
    public TaskPipelineBuilder<TaskContexts.PreEnvironment, TaskContexts.HostReady> PipelineBuilder { get; }
    
    private ILogger? _logger;
    private readonly IDisposable _subscription;

    public UniScanServerHostInitializationPipeline()
    {
        PipelineBuilder = TaskPipelineBuilder<TaskContexts.PreEnvironment>.Create()
                      .ThenRun((_, _) =>
                       {
                           _logger = Log.Logger.ForContext<UniScanServerHostInitializationPipeline>();
                       })
                      .ThenRunAsync(Initializers.InitializeEnvironment)
                  .ThenTransitionTo<TaskContexts.PreModules>()
                      .ThenRunAsync(Initializers.InitializeModules)
                  .ThenTransitionTo<TaskContexts.HostReady>()
                      .ThenRun((_, _) =>
                       {
                           _subscription?.Dispose();
                       });

        _subscription = PipelineBuilder.ExecutionState.CurrentStatus.AsObservable().Skip(1).Subscribe(s => { _logger?.Information("{Status}", s); });
    }
    
    public async Task<TaskContexts.HostReady> RunAsync(UniScanServer server, string rootDirectory) => await PipelineBuilder.Build().RunAsync(new TaskContexts.PreEnvironment(server, rootDirectory));
}