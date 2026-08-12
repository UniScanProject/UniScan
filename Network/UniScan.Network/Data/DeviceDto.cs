using System.Diagnostics.CodeAnalysis;
using MessagePack;
using Semver;
using Shiki.Common.Identity;
using UniScan.Network.Formatter.SemVer;

namespace UniScan.Network.Data;

[MessagePackObject]
public record DeviceSpecifications(
    [property: Key(0)] string Model,
    [property: Key(1), MessagePackFormatter(typeof(SemVersionFormatter))] SemVersion? Version
);

[MessagePackObject]
public record DeviceDto(
    [property: Key(0)] Identifier ScannerIdentifier,
    [property: Key(1)] string? DisplayName,
    [property: Key(2), MemberNotNullWhen(true, nameof(DeviceDto.Specs))] bool Connected,
    [property: Key(3)] DeviceSpecifications? Specs
);