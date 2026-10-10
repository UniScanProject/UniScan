using System.Collections.Concurrent;
using DotNetty.Transport.Channels;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Server.Core.UserInterface;

public interface IUISSRManagerMutationProxy
{
    void Subscribe(IChannel channel, IUISSRContextSession session);
    void Unsubscribe(IChannel channel, Identifier id);

    void UnsubscribeAll(IChannel channel);

    void RegisterFactory(IUISSRContextSessionFactory factory);
    void UnregisterFactory(Identifier id);

    void RegisterNodeProvider(IUINodeProvider provider);
    void UnregisterNodeProvider(IUINodeProvider provider);

    IUINode? ResolveNode(object context);
}

public interface IUISSRManager : IUISSRManagerMutationProxy
{
    public IReadOnlyDictionary<IChannel, ConcurrentDictionary<Identifier, IUISSRContextSession>> ChannelSessions
    {
        get;
    }

    public IReadOnlyDictionary<Identifier, IUISSRContextSessionFactory> SessionFactories { get; }
}

public class UISSRManager : IUISSRManager
{
    private ConcurrentDictionary<IChannel, ConcurrentDictionary<Identifier, IUISSRContextSession>>
        _channelSessions = [];

    public IReadOnlyDictionary<IChannel, ConcurrentDictionary<Identifier, IUISSRContextSession>> ChannelSessions =>
        _channelSessions;

    private readonly ConcurrentDictionary<Identifier, IUISSRContextSessionFactory> _sessionFactories = [];
    public IReadOnlyDictionary<Identifier, IUISSRContextSessionFactory> SessionFactories => _sessionFactories;

    private readonly List<IUINodeProvider> _nodeProviders = [];

    public UISSRManager(IEnumerable<IUISSRContextSessionFactory> factories, IEnumerable<IUINodeProvider> nodeProviders)
    {
        foreach (IUISSRContextSessionFactory factory in factories)
        {
            _sessionFactories.TryAdd(factory.ContextIdentifier, factory);
        }

        _nodeProviders.AddRange(nodeProviders);
    }

    void IUISSRManagerMutationProxy.Subscribe(IChannel channel, IUISSRContextSession session)
    {
        var sessions =
            _channelSessions.GetOrAdd(channel, _ => new ConcurrentDictionary<Identifier, IUISSRContextSession>());
        sessions[session.ContextIdentifier] = session;
    }

    void IUISSRManagerMutationProxy.Unsubscribe(IChannel channel, Identifier id)
    {
        if (!_channelSessions.TryGetValue(channel, out var sessions)) return;

        sessions.TryRemove(id, out IUISSRContextSession? session);
        session?.Dispose();
    }

    void IUISSRManagerMutationProxy.UnsubscribeAll(IChannel channel)
    {
        if (_channelSessions.TryRemove(channel, out var sessions))
        {
            foreach (var session in sessions)
            {
                session.Value.Dispose();
            }
        }
    }

    void IUISSRManagerMutationProxy.RegisterFactory(IUISSRContextSessionFactory factory)
    {
        _sessionFactories.TryAdd(factory.ContextIdentifier, factory);
    }

    void IUISSRManagerMutationProxy.UnregisterFactory(Identifier id)
    {
        _sessionFactories.TryRemove(id, out _);
    }

    public void RegisterNodeProvider(IUINodeProvider provider)
    {
        _nodeProviders.Add(provider);
    }

    public void UnregisterNodeProvider(IUINodeProvider provider)
    {
        _nodeProviders.Remove(provider);
    }

    public IUINode? ResolveNode(object context) => _nodeProviders.Where(p => p.CanProvide(context))
                                                                 .OrderByDescending(p => p.Priority)
                                                                 .FirstOrDefault()?.Provide(context);
}

/// <summary>
/// Creates SSR context sessions
/// 
/// Supposed to be registered to DI for easy construction
/// </summary>
public interface IUISSRContextSessionFactory
{
    Identifier ContextIdentifier { get; }

    /// <summary>
    /// Returns true if the provided args are supported
    /// </summary>
    /// <param name="args">The args</param>
    /// <returns>Whether the args are supported</returns>
    public bool SupportsArgs(object? args);

    /// <summary>
    /// Creates a session
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="contextIdentifier"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    IUISSRContextSession Create(IChannel channel, Identifier contextIdentifier, object? args);
}

/// <summary>
/// Creates SSR context sessions
/// 
/// Supposed to be registered to DI for easy construction
/// </summary>
public interface IUISSRContextSessionFactory<in TArgs> : IUISSRContextSessionFactory
    where TArgs : class
{
    /// <summary>
    /// Creates a session
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="contextIdentifier"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    IUISSRContextSession Create(IChannel channel, Identifier contextIdentifier, TArgs args);

    /// <inheritdoc/>
    bool IUISSRContextSessionFactory.SupportsArgs(object? args) => args is TArgs;

    /// <inheritdoc />
    IUISSRContextSession IUISSRContextSessionFactory.
        Create(IChannel channel, Identifier contextIdentifier, object? args) =>
        Create(channel, contextIdentifier, args as TArgs ?? throw new InvalidCastException());
}

/// <summary>
/// Ongoing session for subscribed context
/// </summary>
public interface IUISSRContextSession : IDisposable
{
    Identifier ContextIdentifier { get; }

    /// <summary>
    /// Initializes the session
    ///
    /// Supposed to set up observing/periodic updates when needed
    /// </summary>
    Task InitializeAsync();
}

/// <summary>
/// Constructs IUINodes based on a given type
/// </summary>
public interface IUINodeProvider : IProvider<IUINode, object?>
{
    /// <summary>
    /// Type this Provider applies to (null for globally usable)
    /// </summary>
    public Type? Type { get; }
}

/// <summary>
/// Constructs IUINodes based on a given type
/// </summary>
public interface IUINodeProvider<in T> : IProvider<IUINode, object?, T>, IUINodeProvider
    where T : class
{
    /// <inheritdoc/>
    Type IUINodeProvider.Type => typeof(T);

    /// <inheritdoc/>
    bool IProvider<IUINode, object?>.CanProvide(object? o) => o is T t && CanProvide(t);

    /// <inheritdoc/>
    IUINode IProvider<IUINode, object?>.Provide(object? o) => Provide((T)o!);
}

/// <summary>
/// For providing updates to IUINodes/slots
/// </summary>
public interface IUINetworkNodeUpdater<in TArgs>
    where TArgs : class
{
    public Task UpdateAsync(IChannel channel, TArgs args);
}