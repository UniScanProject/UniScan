using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using UniScan.Network;

namespace UniScan.Client.App.UI.ConnectionMethod;

public static class ConnectionMethodFactoryViewModelSource
{
    public record ConnectionMethodViewModelFactoryPair(Type Type, ConnectionMethodFactoryViewModelAttribute Attribute);

    public static IEnumerable<ConnectionMethodViewModelFactoryPair> Get()
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
                        .Where(t => t.IsAssignableTo(typeof(IConnectionMethodFactoryViewModel)))
                        .Select(t => new { Type = t, Attribute = t!.GetCustomAttribute<ConnectionMethodFactoryViewModelAttribute>() })
                        .Where(x => x.Attribute != null)
                        .Select(t => new ConnectionMethodViewModelFactoryPair(t.Type, t.Attribute!));
    }
}