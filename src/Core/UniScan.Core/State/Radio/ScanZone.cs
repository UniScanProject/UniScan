namespace UniScan.Core.State.Radio;

public interface IScanZone : IScanNode
{
    public IEnumerable<IScanGroup> BaseGroups { get; }
}

public interface IScanZone<TGroup, TChannel> : IScanZone
where TGroup : IScanGroup<TChannel>
where TChannel : ScanChannel
{
    public List<TGroup> Groups { get; }
}