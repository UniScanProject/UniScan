using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace UniScan.Network.Registry.Source.Sources;

public class AssembliesPacketSource : IPacketSource
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<Type, RegistryPacketAttribute> GetPacketTypes()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
                              .ToDictionary(a => a.GetName().FullName);

        var referenced =
            DependencyContext.Default?.RuntimeLibraries.SelectMany(l => l.GetDefaultAssemblyNames(DependencyContext
                                                                      .Default)) ?? [];

        foreach (AssemblyName assembly in referenced)
        {
            if (!loaded.ContainsKey(assembly.FullName))
            {
                try
                {
                    Assembly.Load(assembly);
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly =>
                         {
                             try
                             {
                                 return assembly.GetTypes();
                             }
                             catch (ReflectionTypeLoadException ex)
                             {
                                 return ex.Types.Where(t => t != null).Select(t => t!); 
                             }
                             catch
                             {
                                 return [];
                             }
                         })
                        .Where(t => t.IsAssignableTo(typeof(IPacket)))
                        .Select(t => new { Type = t, Attribute = t!.GetCustomAttribute<RegistryPacketAttribute>() })
                        .Where(x => x.Attribute != null)
                        .ToDictionary(t => t.Type, t => t.Attribute!);
    }
}