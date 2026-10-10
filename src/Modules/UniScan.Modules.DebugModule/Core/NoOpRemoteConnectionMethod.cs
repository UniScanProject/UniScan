using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Local;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Modules.DebugModule.Core;

[PolymorphicSerializable<IRemoteConnectionMethod>("No-Op")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class NoOpRemoteConnectionMethod(string name) : IRemoteConnectionMethod
{
    [JsonPropertyName("name")] public string Name { get; } = name;
    
    public void Apply(Bootstrap bootstrap)
    {
    }

    public Task<IChannel> ConnectAsync(Bootstrap bootstrap) => Task.FromResult<IChannel>(new LocalChannel());

    public string ToDisplayString() => Name;
}