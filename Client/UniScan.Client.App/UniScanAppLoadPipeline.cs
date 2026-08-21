using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Serilog;
using UniScan.Client.App.Pipeline;
using UniScan.Client.Core;

namespace UniScan.Client.App;

public class UniScanAppLoadPipeline
{
    internal static class LoadContexts
    {
        public class Early : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new("Loading...");

            public ServiceCollection ServiceCollection { get; } = new();
        }

        public class PreClient(string status, ServiceCollection serviceCollection) : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient? Client { get; set; } = null;

            public PreClient(Early ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection)
            {
            }
        }

        public class PostClient(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client)
            : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient Client { get; } = client;

            public PostClient(PreClient ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection,
                                                    ctx.Client ?? throw new NullReferenceException())
            {
            }
        }

        public class PreServiceProvider(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client)
            : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient Client { get; } = client;

            public IServiceProvider? Services { get; set; } = null;

            public PreServiceProvider(PostClient ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection,
                                                             ctx.Client ?? throw new NullReferenceException())
            {
            }
        }

        public class PostServiceProvider(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client,
            IServiceProvider provider)
            : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient Client { get; } = client;

            public IServiceProvider Services { get; } = provider;

            public PostServiceProvider(PreServiceProvider ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection,
                                                                      ctx.Client ?? throw new NullReferenceException(),
                                                                      ctx.Services ??
                                                                      throw new NullReferenceException())
            {
            }
        }
    }

    public TaskPipeline Pipeline { get; }
    private ILogger? _logger = null;
    private IDisposable _subscription;

    public UniScanAppLoadPipeline(UniScanApp app)
    {
        Pipeline = new TaskPipelineBuilder<LoadContexts.Early>()
                  .ThenRun(app.InitializeEnvironment)
                  .ThenRun((ctx) =>
                   {
                       _logger = Log.ForContext<UniScanAppLoadPipeline>();
        
                       return Task.CompletedTask;
                   })
                  .ThenRun(app.InitializeSoftwareInfo)
                  .ThenRun(app.InitializeModules)
                  .ThenTransitionTo<LoadContexts.PreClient>(ctx => new(ctx))
                  .ThenRun(app.InitializeClient)
                  .ThenTransitionTo<
                       LoadContexts.PostClient>(ctx => new(ctx))
                  .ThenRun(app.InitializeUI)
                  .ThenTransitionTo<
                       LoadContexts.PreServiceProvider>(ctx => new(ctx))
                  .ThenRun(app.InitializeServiceProvider)
                  .ThenTransitionTo<
                       LoadContexts.PostServiceProvider>(ctx => new(ctx))
                  .ThenRun(app.InitializeRemotes)
                  .ThenRun(app.FinishInitialization)
                  .ThenRun((ctx) =>
                   {
                       _subscription?.Dispose();
        
                       return Task.CompletedTask;
                   })
                  .Build();
        
        _subscription = Pipeline.Status.Subscribe(s =>
        {
            _logger?.Information("{Status}", s);
        });
    }

    public async Task RunAsync()
    {
        await Pipeline.RunAsync(new LoadContexts.Early());
    }
}