using UniScan.Core.State.Types;

namespace UniScan.Core.State.Radio;

public interface IScanZone : IScanNode
{
    public IEnumerable<IScanGroup> BaseGroups { get; }
}

public interface IScanZone<TGroup, TChannel> : IScanZone
where TGroup : IScanGroup<TChannel>
where TChannel : IScanChannel
{
    public List<TGroup> Groups { get; }
}

public record ScanZone<TGroup, TChannel>(string? Name, AvoidStatus AvoidStatus, bool Holding, List<TGroup> Groups)
    : IScanZone<TGroup, TChannel>
    where TGroup : IScanGroup<TChannel>
    where TChannel : IScanChannel
{
    public IEnumerable<IScanGroup> BaseGroups => Groups.Cast<IScanGroup>();
}