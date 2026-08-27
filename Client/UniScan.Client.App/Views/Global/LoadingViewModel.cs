using Shiki.TaskPipeline;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Global;

public class LoadingViewModel : ViewModelBase
{
    public static string VersionString => $"UniScan Client v{UniScanApp.SoftwareInfo.Version} (Platform v{UniScanApp.PlatformVersion})";
    
    public TaskPipeline TaskPipeline { get; }
    public string LoadingText { get; }

    public LoadingViewModel(string loadingText, TaskPipeline taskPipeline)
    {
        LoadingText = loadingText;
        TaskPipeline = taskPipeline;
    }
}