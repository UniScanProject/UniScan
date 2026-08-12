using UniScan.Platform.Filesystem;

namespace UniScan.Platform;

public record HostEnvironment(
    IPlatformStandardPaths StandardPaths,
    IPlatformSerilogInitializer SerilogInitializer,
    IPlatformDirectoryManager DirectoryManager,
    IPlatformFileManager FileManager);