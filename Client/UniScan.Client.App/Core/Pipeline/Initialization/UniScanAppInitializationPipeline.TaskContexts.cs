using System;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Shiki.TaskPipeline;
using UniScan.Client.Core;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
    internal static partial class TaskContexts
    {
        public class Early : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new("Loading...");

            public ServiceCollection ServiceCollection { get; } = new();
        }

        public class PreClient(string status, ServiceCollection serviceCollection) : ITaskContext<PreClient, Early>
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient? Client { get; set; } = null;

            public PreClient(Early ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection)
            {
            }

            public static PreClient TransitionFrom(Early oldContext) => new(oldContext);
        }

        public class PostClient(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client)
            : ITaskContext<PostClient, PreClient>
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient Client { get; } = client;

            public PostClient(PreClient ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection,
                                                    ctx.Client ?? throw new NullReferenceException())
            {
            }

            public static PostClient TransitionFrom(PreClient oldContext) => new(oldContext);
        }

        public class PreServiceProvider(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client)
            : ITaskContext<PreServiceProvider, PostClient>
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public ServiceCollection ServiceCollection { get; } = serviceCollection;

            public UniScanClient Client { get; } = client;

            public IServiceProvider? Services { get; set; } = null;

            public PreServiceProvider(PostClient ctx) : this(ctx.Status.CurrentValue, ctx.ServiceCollection,
                                                             ctx.Client ?? throw new NullReferenceException())
            {
            }

            public static PreServiceProvider TransitionFrom(PostClient oldContext) => new(oldContext);
        }

        public class PostServiceProvider(
            string status,
            ServiceCollection serviceCollection,
            UniScanClient client,
            IServiceProvider provider)
            : ITaskContext<PostServiceProvider, PreServiceProvider>
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

            public static PostServiceProvider TransitionFrom(PreServiceProvider oldContext) => new(oldContext);
        }
    }
}