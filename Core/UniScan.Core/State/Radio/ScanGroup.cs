using UniScan.Core.State.Types;

namespace UniScan.Core.State.Radio;

public record ScanGroup<TChannel>(string? Name, AvoidStatus AvoidStatus, bool Holding, List<TChannel> Channels) : IScanGroup<TChannel>
where TChannel : ScanChannel;