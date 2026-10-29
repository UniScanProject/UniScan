using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Implementations.Assembly;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization;
using Shiki.Internationalization.Component.Resolver;
using Shiki.Internationalization.Translation;
using Shiki.Internationalization.Translation.LanguageSource;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;
using UniScan.Client.App.Core.Module.Modules.Internal.Options;
using UniScan.Client.App.Core.TextComponent.Resolver;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views;
using UniScan.Client.App.Views.Settings;
using UniScan.Client.Core;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.SSR;
using UniScan.Core.State;
using UniScan.Network;
using UniScan.Network.Group;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Platform;
using MainViewModel = UniScan.Client.App.Views.MainViewModel;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
    public static class Initializers
    {
        public static async Task InitializeEnvironment(TaskContexts.PreEnvironment ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing environment";

            if (ctx.StandardPaths      == null || ctx.DirectoryManager == null || ctx.FileManager == null ||
                ctx.SerilogInitializer == null)
                throw new NullReferenceException("Platform environment fields have not been set.");
            
            await ctx.StandardPaths.CreateAllAsync(ctx.DirectoryManager);
            Log.Logger = ctx.SerilogInitializer.GetConfiguration().CreateLogger().ForContext<UniScanApp>();
        
            ctx.ServiceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
            Log.Debug("Initialized Environment");
        
#if DEBUG
            Log.Debug("Data path: {Path}", ctx.StandardPaths.DataPath);
            Log.Debug("Config path: {Path}", ctx.StandardPaths.ConfigPath);
            Log.Debug("Cache path: {Path}", ctx.StandardPaths.CachePath);
#endif
        }
        
        public static void InitializeSettingsBuilder(UniScanAppInitializationPipeline.TaskContexts.PreSettings ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing Settings builder";

            //nothing to do here for now
        }

        public static void SetupClientMessagePackResolvers(TaskContexts.PreSettings ctx,
                                                                 ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing client MessagePack resolvers";

            IFormatterResolver resolver = CompositeResolver.Create(
                                                                   [
                                                                   ],
                                                                   [
                                                                       MessagePackSerializer.DefaultOptions.Resolver,
                                                                   ]
                                                                  );

            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(resolver);
        }

        public static void InitializeSoftwareInfo(UniScanAppInitializationPipeline.TaskContexts.PreSettings ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing SoftwareInfo";

            ctx.ServiceCollection.AddSingleton(UniScanApp.SoftwareInfo);
            Log.Information("{Info}", UniScanApp.SoftwareInfo);
        }
        
        public static void SetupBindableRegistry(TaskContexts.PreSettings ctx,
                                                         ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing bindables";

            ctx.ServiceCollection.AddSingleton<IUIBindingsRegistry, UIBindingsRegistry>();
        }

        public static async Task<TaskContexts.PreServiceProvider> SetupTranslations(TaskContexts.PreTranslations ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing translations";

            TranslationManager mgr = new();
            await mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                         new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N",
                                                             new EmbeddedDirectoryManager(Assembly
                                                                .GetExecutingAssembly()),
                                                             new EmbeddedFileManager(Assembly
                                                                .GetExecutingAssembly()),
                                                             new JsonStorageSerializer<LanguageFileDto>())
                                                        ), "app".ToSlug<SnakeSlugFormatter>(), 20);
            await mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                         new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N",
                                                             new EmbeddedDirectoryManager(typeof(IDeviceState)
                                                                .Assembly),
                                                             new EmbeddedFileManager(typeof(IDeviceState)
                                                                .Assembly),
                                                             new JsonStorageSerializer<LanguageFileDto>())
                                                        ), "core".ToSlug<SnakeSlugFormatter>(), 20);
            mgr.UseLanguage("en-US");

            ctx.ServiceCollection.AddSingleton<TranslationManager>(mgr);
            ctx.TranslationManager = mgr;

            return new TaskContexts.PreServiceProvider(ctx.UniScanApp, ctx.ServiceCollection, mgr,
                                                       ctx.PlatformStandardPaths, ctx.PlatformFileManager,
                                                       ctx.PlatformDirectoryManager, ctx.SettingsManager);
        }

        public static void InitializeNetworkGroupManager(TaskContexts.PreServiceProvider ctx,
                                                         ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing network group manager";
            
            NetworkOptionGroup? s = ctx.SettingsManager.TryGetGroup<NetworkOptionGroup>();
            if (s != null && !string.IsNullOrWhiteSpace(s.NetworkGroupMethod))
            {
                Type? t = PolymorphicTypeRegistry<INetworkGroupManager>.Instance.GetType(s.NetworkGroupMethod);
                if (t != null)
                {
                    Log.Information("Using network group method '{Key}' ({Type})", s.NetworkGroupMethod, t.FullName);
                    ctx.ServiceCollection.AddSingleton(typeof(INetworkGroupManager), t);
                    return;
                }
                
                Log.Error("Network group method '{Key}' not found, defaulting to 'Multithreaded'", s.NetworkGroupMethod);
            }
            else
            {
                Log.Error("Empty/No network method provided, defaulting to 'Multithreaded'");
            }

            ctx.ServiceCollection.AddSingleton<INetworkGroupManager, MultithreadedNetworkGroupManager>();
        }        
        
        public static void SetupTextComponentResolver(TaskContexts.PreServiceProvider ctx,
                                                              ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing text component resolver";

            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, FallbackTextComponentResolver>();
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, AbsoluteTextComponentResolver>();
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, BoundTextComponentResolver>();
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, TranslatableTextComponentResolver>();

            ctx.ServiceCollection.AddSingleton<TextComponentResolverService>();
        }

        public static void SetupSSRSlotRegistry(TaskContexts.PreSettings ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing serverside UI rendering slot registry";
            ctx.ServiceCollection.AddSingleton<IUISlotRegistry, UISlotRegistry>();
        }
        
        public static void SetupSSRUI(TaskContexts.PreServiceProvider ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing serverside UI rendering";

            //modules have been loaded by this point and DI has been initialized, now we set up our ssr ui factories
            ctx.ServiceCollection.AddSingleton<IUIViewFactory, UIViewFactory>();
        }

        internal static async Task InitializeClient(TaskContexts.PreServiceProvider ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing client";
        
            await using ServiceProvider serviceProvider = ctx.ServiceCollection.BuildServiceProvider();
            IPlatformStandardPaths standardPaths = serviceProvider.GetRequiredService<IPlatformStandardPaths>();
            IPlatformDirectoryManager directoryManager = serviceProvider.GetRequiredService<IPlatformDirectoryManager>();
            IPlatformFileManager fileManager = serviceProvider.GetRequiredService<IPlatformFileManager>();
        
            await ctx.ServiceCollection.AddUniScanClientAsync(standardPaths, fileManager, directoryManager, UniScanApp.SoftwareInfo);
        }

        public static void InitializeViews(TaskContexts.PreServiceProvider ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing UI";

            ctx.ServiceCollection.AddSingleton<MainView>();
            ctx.ServiceCollection.AddSingleton<MainViewModel>();

            ctx.ServiceCollection.AddSingleton<ClientSettingsViewModel>();
        }

        public static TaskContexts.UniScanAppReady InitializeServiceProvider(TaskContexts.PreServiceProvider ctx,
                                                     ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Building ServiceProvider";

            return new TaskContexts.UniScanAppReady(ctx.UniScanApp, ctx.SettingsManager, ctx.ServiceCollection.BuildServiceProvider());
        }

        public static async Task InitializeRemotes(TaskContexts.UniScanAppReady ctx,
                                                     ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Loading remotes";

            IRemoteStorage remoteStorage = ctx.ServiceProvider.GetRequiredService<IRemoteStorage>();
            await remoteStorage.LoadAsync();

            IRemoteManager remoteManager = ctx.ServiceProvider.GetRequiredService<IRemoteManager>();
            Log.Information("Loaded {Count} remote(s)", remoteManager.Remotes.Count);
        }

        public static void RegisterPackets(TaskContexts.UniScanAppReady ctx,
                                      ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Registering packets";

            PacketRegistry registry = ctx.ServiceProvider.GetRequiredService<PacketRegistry>();
            registry.RegisterFromSource<AssembliesPacketSource>();
        }
    }
}