using System;
using System.Threading.Tasks;
using R3;
using Serilog;
using UniScan.Client.App.Core.Pipeline;

namespace UniScan.Client.App.Core.Initialization;

public partial class UniScanAppInitializationPipeline
{
    public TaskPipeline Pipeline { get; }
    private ILogger? _logger = null;
    private readonly IDisposable _subscription;

    public UniScanAppInitializationPipeline(UniScanApp app)
    {
        Pipeline = new TaskPipelineBuilder<TaskContexts.Early>()
                   //early
                      .ThenRun(app.InitializeEnvironment)
                      .ThenRun(_ =>
                       {
                           _logger = Log.ForContext<UniScanAppInitializationPipeline>();

                           return Task.CompletedTask;
                       })
                      .ThenRun(app.InitializeSoftwareInfo)
                      .ThenRun(app.InitializeModules)
                  .ThenTransitionTo<TaskContexts.PreClient>()
                    .ThenRun(app.InitializeClient)
                  .ThenTransitionTo<TaskContexts.PostClient>()
                    .ThenRun(InitializeViews)
                  .ThenTransitionTo<TaskContexts.PreServiceProvider>()
                    .ThenRun(InitializeServiceProvider)
                  .ThenTransitionTo<TaskContexts.PostServiceProvider>()
                      .ThenRun(InitializeRemotes)
                      .ThenRun(app.FinishInitialization)
                      .ThenRun(_ =>
                       {
                           _subscription?.Dispose();

                           return Task.CompletedTask;
                       })
                  .Build();

        _subscription = Pipeline.Status.Subscribe(s => { _logger?.Information("{Status}", s); });
    }

    public async Task RunAsync()
    {
        await Pipeline.RunAsync(new TaskContexts.Early());
    }
}