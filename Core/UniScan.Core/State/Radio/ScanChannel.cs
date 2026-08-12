using UniScan.Core.State.Types;

namespace UniScan.Core.State.Radio;

public interface IScanChannel : IScanNode
{
    
}

public abstract record ScanChannel(string? Name, AvoidStatus AvoidStatus, bool Holding) : IScanChannel;