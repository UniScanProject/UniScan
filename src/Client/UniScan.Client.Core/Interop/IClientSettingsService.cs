using R3;
using UniScan.Client.Core.Config;

namespace UniScan.Client.Core.Interop;

public interface IClientSettingsService
{
    Observable<ClientSettings> Settings { get; }
}