using System.Text.Json;
using R3;
using Serilog;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.TaskPipeline.Builder;
using Shiki.Settings;
using UniScan.Server.Core.Module;

namespace UniScan.Server.Core.Pipeline;

public partial class UniScanServerInitializationPipeline
{
    public TaskPipelineBuilder<TaskContexts.PreEnvironment, TaskContexts.ServerReady> PipelineBuilder { get; }
    
    private ILogger? _logger;
    private readonly IDisposable _subscription;

    public UniScanServerInitializationPipeline(UniScanServer server)
    {
        PipelineBuilder = TaskPipelineBuilder<TaskContexts.PreEnvironment>.Create()
                      .ThenRun(server.InitializeServer)
                      .ThenRun((_, _) =>
                       {
                           _logger = Log.ForContext<UniScanServerInitializationPipeline>();
                       })
                  .ThenTransitionTo<TaskContexts.PreSettings>()//settingsbuilder created in here
                      .ThenRunAsync(server.InitializeSettingsAsync)
                  .ThenTransitionTo<TaskContexts.PreServiceProvider>()
                      .ThenRunAsync(Initializers.InitializeTranslationsAsync)
                      .ThenRun(server.ConfigureModules)
                      .ThenRun(server.InitializeRemoteInfoManager)
                      .ThenRun(Initializers.InitializeTextComponentResolver)
                      .ThenRun(Initializers.InitializeSSR)
                      .ThenRun(Initializers.InitializeNetworkGroupManager)
                  .ThenRunToTransitionTo<TaskContexts.ServerReady>(server.InitializeServiceProvider)
                      .ThenRun(server.InitializeNetworking)
                      .ThenRun(server.FinishInitialization)
                      .ThenRun((_, _) =>
                       {
                           _subscription?.Dispose();
                       });

        _subscription = PipelineBuilder.ExecutionState.CurrentStatus.AsObservable().Skip(1).Subscribe(s => { _logger?.Information("{Status}", s); });
    }
    
    public async Task RunAsync(TaskContexts.PreEnvironment initCtx) => await PipelineBuilder.Build().RunAsync(initCtx);
}