using UniScan.Core.State.Types;

namespace UniScan.Core.State.Radio;

public record ScanList(string Name, AvoidStatus AvoidStatus, bool Holding, List<IScanZone> Zones) : IScanNode;