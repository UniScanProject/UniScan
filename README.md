# UniScan

![GitHub Release](https://img.shields.io/github/v/release/UniScanProject/UniScan?sort=date&filter=UniScan.Client.App%2F*&label=Client)
![GitHub Release](https://img.shields.io/github/v/release/UniScanProject/UniScan?sort=date&filter=UniScan.Server.Host%2F*&display_name=tag&label=Server)
---

Modular scanner/radio client and multi-broadcast server. Plug in a scanner, set it up in the server software, and open
it up to the internet for others to listen to, and control it yourself by logging into an admin user.

## Subprojects

|                                              Name                                               | Version | Description                                    |
|:-----------------------------------------------------------------------------------------------:|:-------:|------------------------------------------------|
| [UniScan.Server.Core](src/Server/UniScan.Server.Core) | 0.3.0 | Shared server code |
| [UniScan.Server.Host](src/Server/UniScan.Server.Host) | 0.1.1 | Bootstraps UniScan.Server.Core |
| [UniScan.Server.Authentication](src/Server/UniScan.Server.Authentication) | 0.0.1 | Common auth code and storage |
| [UniScan.Platform](src/Platform/UniScan.Platform) | 0.1.0 | Shared platform abstractions |
| [UniScan.Platform.DependencyInjection](src/Platform/UniScan.Platform.DependencyInjection) | 0.0.1 | Adds some DI extensions to UniScan.Platform |
| [UniScan.Platform.Implementations.Native](src/Platform/UniScan.Platform.Implementations.Native) | 0.0.3 | Platform implementations for native environments |
| [UniScan.Platform.Implementations.Web](src/Platform/UniScan.Platform.Implementations.Web) | 0.1.0 | Platform implementations for the restrictive WASM environment |
| [UniScan.Network](src/Network/UniScan.Network) | 0.3.0 | Contains the protocol and shared networking code |
| [UniScan.Network.Client](src/Network/UniScan.Network.Client) | 0.0.2 | Networking code specific to the client |
| [UniScan.Network.Server](src/Network/UniScan.Network.Server) | 0.0.2 | Networking code specific to the server |
| [UniScan.Network.CodeGenerator](src/Network/UniScan.Network.CodeGenerator) | 0.0.1 | Codegen for UniScan.Network |
| [UniScan.Core](src/Core/UniScan.Core) | 0.0.2 |  |
| [UniScan.Device](src/Core/UniScan.Device) | 0.3.0 |  |
| [UniScan.UserInterface](src/Core/UniScan.UserInterface) |  |  |
| [UniScan.Client.Core](src/Client/UniScan.Client.Core) | 0.3.0 | Holds shared client code, no dep on Avalonia |
| [UniScan.Client.App](src/Client/UniScan.Client.App) | 0.1.1 | UniScan client |
| [UniScan.Client.App.Platform.Android](src/Client/UniScan.Client.App.Platform.Android) | 0.0.4 | UniScan client for Android |
| [UniScan.Client.App.Platform.Browser](src/Client/UniScan.Client.App.Platform.Browser) | 0.1.3 | UniScan client for web browsers (https://uniscan.dexrn.me) |
| [UniScan.Client.App.Platform.Desktop](src/Client/UniScan.Client.App.Platform.Desktop) | 0.0.3 | UniScan client for desktop |
| [UniScan.Client.App.Platform.iOS](src/Client/UniScan.Client.App.Platform.iOS) |  | UniScan client for iOS |
| [UniScan.Modules.DebugModule](src/Modules/UniScan.Modules.DebugModule) | 0.0.1 |  |