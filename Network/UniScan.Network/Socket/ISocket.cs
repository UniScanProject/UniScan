using System.Data;
using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Event;
using Shiki.Common.Result;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Socket.Configuration;
using UniScan.Network.Util;

namespace UniScan.Network.Socket;

public interface ISocket
{
    protected ILogger Logger { get; }
    
    protected UniScanChannelInitializer ChannelInitializer { get; }

    Task StartAsync();
    Task StopAsync();
    
    Task<bool> SendPacketAsync(IChannel? channel, IPacket packet);

    public Task<Result<TResponse, Exception>> SendRequestAsync<TResponse>(IChannel? channel, IRequestPayloadPart<TResponse> packet)
        where TResponse : IPacket, IResponsePayloadPart;
}