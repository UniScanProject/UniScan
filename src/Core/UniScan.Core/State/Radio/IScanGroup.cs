namespace UniScan.Core.State.Radio;

public interface IScanGroup : IScanNode
{
    IEnumerable<IScanChannel> BaseChannels { get; }
}

public interface IScanGroup<TChannel> : IScanGroup
where TChannel : IScanChannel
{
    List<TChannel> Channels { get; }
}