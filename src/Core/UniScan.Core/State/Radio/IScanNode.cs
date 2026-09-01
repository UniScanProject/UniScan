using UniScan.Core.State.Types;

namespace UniScan.Core.State.Radio;

public interface IScanNode
{
    public string? Name { get; }
    public AvoidStatus AvoidStatus { get; }
    public bool Holding { get; }
}