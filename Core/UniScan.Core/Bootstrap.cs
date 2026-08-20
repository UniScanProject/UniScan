using System.Runtime.CompilerServices;
using MessagePack;
using MessagePack.Resolvers;
using Serilog;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;

namespace UniScan.Core;

public static class Bootstrap
{
    private static readonly ILogger _logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: Constants.ConsoleOutputTemplate).CreateLogger().ForContext(typeof(Bootstrap));

    [ModuleInitializer]
    public static void InitializeMessagePack()
    {
        _logger.Information("Initializing MessagePack Resolvers");

        IFormatterResolver resolver = CompositeResolver.Create(
                                                               SlugMessagePackFormatterResolver.Instance,
                                                               StandardResolver.Instance
                                                              );

        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}