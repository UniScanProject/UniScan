using UniScan.Client.App.Core.Pipeline;

namespace UniScan.Client.App.ViewModels;

public class LoadingViewModel : ViewModelBase
{
    public static string VersionString => $"UniScan Client v{UniScanApp.SoftwareInfo.Version} (Platform v{UniScanApp.PlatformVersion})";
    
    public TaskPipeline TaskPipeline { get; }

    public LoadingViewModel(TaskPipeline taskPipeline)
    {
        TaskPipeline = taskPipeline;
    }
}