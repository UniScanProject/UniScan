using System;
using R3;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.Core.Pipeline.Connection;

public partial class RemoteConnectionPipeline
{
    public static partial class TaskContexts
    {
        public class ConnectionContext(IServiceProvider provider, RemoteServer server) : ITaskContext
        {
            public BindableReactiveProperty<string> Status { get; } = new("Connecting...");

            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
        }
        
        public class NegotiationContext(string status, IServiceProvider provider, RemoteServer server) : ITaskContext<NegotiationContext, ConnectionContext>
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
            
            public static NegotiationContext TransitionFrom(ConnectionContext oldContext) => new(oldContext.Status.Value, oldContext.ServiceProvider, oldContext.RemoteServer);
        }
        
        public class RemoteContext(string status, IServiceProvider provider, RemoteServer server) : ITaskContext<RemoteContext, NegotiationContext>
        {
            public BindableReactiveProperty<string> Status { get; } = new(status);

            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
            
            public static RemoteContext TransitionFrom(NegotiationContext oldContext) => new(oldContext.Status.Value, oldContext.ServiceProvider, oldContext.RemoteServer);
        }
    }
}