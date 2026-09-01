using System.Collections;
using UniScan.Platform.Filesystem;

namespace UniScan.Platform;

public interface IPlatformStandardPaths : IEnumerable<string>
{
    public string DataPath { get; }
    public string ConfigPath { get; }
    public string CachePath { get; }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        yield return DataPath;
        yield return ConfigPath;
        yield return CachePath;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public async Task CreateAllAsync(IPlatformDirectoryManager directoryManager)
    {
        foreach (string p in this)
        {
            await directoryManager.CreateDirectoryAsync(p);
        }
    }
}