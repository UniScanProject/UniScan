using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Registry;
using Shiki.FileSystem.Implementations.Assembly;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization;
using Shiki.Internationalization.Component.Resolver;
using Shiki.Internationalization.Translation;
using Shiki.Internationalization.Translation.LanguageSource;
using Shiki.TaskPipeline.Context;
using UniScan.Core.State;
using UniScan.Network.Group;
using UniScan.Server.Core.UserInterface;

namespace UniScan.Server.Core.Pipeline;

public partial class UniScanServerInitializationPipeline
{
    public static class Initializers
    {
        public static async Task InitializeTranslationsAsync(TaskContexts.PreServiceProvider ctx,
                                                        ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing translations";
            
            TranslationManager mgr = new();
            mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                         new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N", EmbeddedDirectoryManager.OfCurrentlyExecuting, EmbeddedFileManager.OfCurrentlyExecuting, new JsonStorageSerializer<LanguageFileDto>())
                                                        ), "server".ToSlug<SnakeSlugFormatter>(), 20).GetAwaiter().GetResult(); 
            mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                         new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N", new EmbeddedDirectoryManager(typeof(IDeviceState).Assembly), new EmbeddedFileManager(typeof(IDeviceState).Assembly), new JsonStorageSerializer<LanguageFileDto>())
                                                        ), "core".ToSlug<SnakeSlugFormatter>(), 20).GetAwaiter().GetResult();
            
            ctx.ServiceCollection.AddSingleton<TranslationManager>(mgr);
        }
        
        public static void InitializeTextComponentResolver(TaskContexts.PreServiceProvider ctx,
                                                             ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing text component resolver";
            
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, FallbackTextComponentResolver>();
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, AbsoluteTextComponentResolver>();
            ctx.ServiceCollection.AddSingleton<ITextComponentResolver, TranslatableTextComponentResolver>();
            
            ctx.ServiceCollection.AddSingleton<TextComponentResolverService>();
        }        
        
        public static void InitializeSSR(TaskContexts.PreServiceProvider ctx,
                                                             ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing Serverside Rendering";
            
            ctx.ServiceCollection.AddSingleton<IUISSRManager, UISSRManager>();
            ctx.ServiceCollection.AddSingleton<IUISSRManagerMutationProxy>(p => p.GetRequiredService<IUISSRManager>());
        } 
        
        public static void InitializeNetworkGroupManager(TaskContexts.PreServiceProvider ctx,
                                                             ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing network group manager";
            
            MainOptionGroup? s = ctx.SettingsManager.TryGetGroup<MainOptionGroup>();
            if (s != null && !string.IsNullOrWhiteSpace(s.NetworkGroupMethod))
            {
                Type? t = PolymorphicTypeRegistry<IHostNetworkGroupManager>.Instance.GetType(s.NetworkGroupMethod);
                if (t != null)
                {
                    Log.Information("Using network group method '{Key}' ({Type})", s.NetworkGroupMethod, t.FullName);
                    ctx.ServiceCollection.AddSingleton(typeof(IHostNetworkGroupManager), t);
                    return;
                }
                
                Log.Error("Network group method '{Key}' not found, defaulting to 'Multithreaded'", s.NetworkGroupMethod);
            }
            else
            {
                Log.Error("Empty/No network method provided, defaulting to 'Multithreaded'");
            }

            ctx.ServiceCollection.AddSingleton<IHostNetworkGroupManager, MultithreadedHostNetworkGroupManager>();
        }        
    }
}