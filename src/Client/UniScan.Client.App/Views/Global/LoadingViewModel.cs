using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Global;

public class LoadingViewModel(string loadingText, IReadOnlyTaskExecutionContext executionContext)
    : ViewModelBase
{
    public IReadOnlyTaskExecutionContext TaskExecutionContext { get; } = executionContext;
    public string LoadingText { get; } = loadingText;
}