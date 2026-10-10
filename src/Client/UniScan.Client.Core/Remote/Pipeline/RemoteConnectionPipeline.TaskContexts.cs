using R3;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;

namespace UniScan.Client.Core.Remote.Pipeline;

public partial class RemoteConnectionPipeline
{
    public static partial class TaskContexts
    {
        public class ConnectionContext(IServiceProvider provider, RemoteServer server) : ITaskContext
        {
            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
        }
        
        public class NegotiationContext(IServiceProvider provider, RemoteServer server) : ITransitionableTaskContext<NegotiationContext, ConnectionContext>
        {
            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
            
            public static NegotiationContext TransitionFrom(ConnectionContext oldContext) => new(oldContext.ServiceProvider, oldContext.RemoteServer);
        }
        
        public class RemoteContext(IServiceProvider provider, RemoteServer server) : ITransitionableTaskContext<RemoteContext, NegotiationContext>
        {
            public RemoteServer RemoteServer { get; } = server;
            public IRemoteServerMutationProxy RemoteServerMutationProxy => RemoteServer;

            public IServiceProvider ServiceProvider { get; } = provider;
            
            public static RemoteContext TransitionFrom(NegotiationContext oldContext) => new(oldContext.ServiceProvider, oldContext.RemoteServer);
        }
    }
}