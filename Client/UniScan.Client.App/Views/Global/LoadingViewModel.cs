using Shiki.TaskPipeline;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Global;

public class LoadingViewModel : ViewModelBase
{
    public TaskPipeline TaskPipeline { get; }
    public string LoadingText { get; }

    public LoadingViewModel(string loadingText, TaskPipeline taskPipeline)
    {
        LoadingText = loadingText;
        TaskPipeline = taskPipeline;
    }
}