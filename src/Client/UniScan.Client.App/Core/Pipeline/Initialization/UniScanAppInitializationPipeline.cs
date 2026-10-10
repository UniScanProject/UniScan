using System;
using System.Threading.Tasks;
using R3;
using Serilog;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Builder;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
    public TaskPipelineBuilder<TaskContexts.PreEnvironment, TaskContexts.UniScanAppReady> PipelineBuilder { get; }
    
    private ILogger? _logger;
    private readonly IDisposable _subscription;

    public UniScanAppInitializationPipeline(UniScanApp app)
    {
        PipelineBuilder = TaskPipelineBuilder<TaskContexts.PreEnvironment>.Create()
                      .ThenRunAsync(Initializers.InitializeEnvironment)
                      .ThenRun((_, _) =>
                       {
                           _logger = Log.ForContext<UniScanAppInitializationPipeline>();
                       })
                  .ThenTransitionTo<TaskContexts.PreTranslations>()
                      .ThenRun(Initializers.SetupClientMessagePackResolvers)
                      .ThenRun(Initializers.InitializeSoftwareInfo)
                      .ThenRun(Initializers.SetupSSRSlotRegistry)
                      .ThenRunAsync(app.InitializeModules)
                      .ThenRun(Initializers.SetupBindableRegistry)
                  .ThenRunToTransitionToAsync<TaskContexts.PreServiceProvider>(Initializers.SetupTranslations)
                      .ThenRun(Initializers.SetupTextComponentResolver)
                      .ThenRun(Initializers.SetupSSRUI)
                      .ThenRunAsync(Initializers.InitializeClient)
                      .ThenRun(Initializers.InitializeViews)
                  .ThenRunToTransitionTo<TaskContexts.UniScanAppReady>(Initializers.InitializeServiceProvider)
                      .ThenRunAsync(Initializers.InitializeRemotes)
                      .ThenRun(Initializers.RegisterPackets)
                      .ThenRunAsync(app.FinishInitialization)
                      .ThenRun((_, _) =>
                       {
                           _subscription?.Dispose();
                       });

        _subscription = PipelineBuilder.ExecutionState.CurrentStatus.AsObservable().Skip(1).Subscribe(s => { _logger?.Information("{Status}", s); });
    }
    
    public async Task RunAsync(UniScanApp app) => await PipelineBuilder.Build().RunAsync(new TaskContexts.PreEnvironment(app));
}