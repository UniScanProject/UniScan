using Microsoft.Extensions.DependencyInjection;
using UniScan.Platform.Filesystem;

namespace UniScan.Platform.DependencyInjection;

public static class HostEnvironmentExtensions
{
    extension(HostEnvironment env)
    {
        public void AddToDi(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPlatformStandardPaths>(env.StandardPaths);
            serviceCollection.AddSingleton<IPlatformSerilogInitializer>(env.SerilogInitializer);
            serviceCollection.AddSingleton<IPlatformDirectoryManager>(env.DirectoryManager);
            serviceCollection.AddSingleton<IPlatformFileManager>(env.FileManager);
        }
    }
}