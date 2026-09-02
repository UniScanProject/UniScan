using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DotNetty.Transport.Channels;
using Shiki.Common.Result;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Socket;

namespace UniScan.Network.Request;

public interface IRequest
{
    bool TrySetResult(IPacket packet);
    bool TrySetException(Exception ex);
}

public class Request<TPacket> : IRequest
    where TPacket : IRequestIdPayloadPart, IPacket
{
    private readonly TaskCompletionSource<TPacket> _tcs = new();
    public Task<TPacket> Task => _tcs.Task;
    
    public bool TrySetResult(IPacket packet)
    {
        if (packet is TPacket p)
        {
            return _tcs.TrySetResult(p);
        }
        
        return _tcs.TrySetException(new InvalidOperationException($"Received packet does not inherit from {typeof(IRequestIdPayloadPart).FullName}, instead got type {typeof(TPacket).FullName}"));
    }
    
    public bool TrySetException(Exception ex) => _tcs.TrySetException(ex);
}

public class RequestManager : IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, IRequest> _pendingRequests = [];

    public static Guid CreateRequestId() => Guid.NewGuid();
    
    public async Task<Result<TResponse, Exception>> MakeRequestAsync<TResponse>(IChannel? channel, IRequestPayloadPart<TResponse> request, CancellationToken ct = default)
    where TResponse : IPacket, IResponsePayloadPart
    {
        if (channel is null)
            return new Result<TResponse, Exception>(new ArgumentNullException(nameof(channel)));
        
        if (request is not IPacket packet)
            return new Result<TResponse, Exception>(new InvalidCastException());
        
        if (HasRequest(request.RequestId))
            return new Result<TResponse, Exception>(new Exception("Request id is already present or null"));
        
        //create req
        var r = new Request<TResponse>();
        _pendingRequests.TryAdd(request.RequestId!.Value, r);
        
        try
        {
            //send
            await channel.WriteAndFlushAsync(packet);
            
            //wait, if packet comes in too quickly I think it should be fine because task will be fulfilled already anyway
            TResponse res = await r.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
            return new Result<TResponse, Exception>(res);
        }
        catch (TimeoutException ex)
        {
            return new Result<TResponse, Exception>(ex);
        }
        catch (InvalidCastException ex)
        {
            return new Result<TResponse, Exception>(ex);
        }
        finally
        {
            _pendingRequests.TryRemove(request.RequestId.Value, out _);
        }
    }

    public bool IsInvalidRequestId(Guid? requestId) => requestId == null || requestId.Value == Guid.Empty;
    public bool HasRequest(Guid? requestId) => !IsInvalidRequestId(requestId) && _pendingRequests.ContainsKey(requestId!.Value);

    public bool TryCompleteRequest(IPacket packet)
    {
        if (packet is not IRequestIdPayloadPart requestPacket)
            return false;

        if (IsInvalidRequestId(requestPacket.RequestId))
            return false;
        
        return _pendingRequests.TryGetValue(requestPacket.RequestId!.Value, out IRequest? tcs) && tcs.TrySetResult(packet);
    }

    public Task RejectAllAsync(Exception exception)
    {
        foreach (var req in _pendingRequests)
        {
            req.Value.TrySetException(exception); 
        }

        return Task.CompletedTask;
    }
    
    public async ValueTask DisposeAsync()
    {
        await RejectAllAsync(new ObjectDisposedException("RequestManager is being disposed"));
        
        this._pendingRequests.Clear();
    }
}