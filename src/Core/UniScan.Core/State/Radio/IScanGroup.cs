namespace UniScan.Core.State.Radio;

public interface IScanGroup : IScanNode
{
    IEnumerable<ScanChannel> BaseChannels { get; }
}

public interface IScanGroup<TChannel> : IScanGroup
where TChannel : ScanChannel
{
    List<TChannel> Channels { get; }
}