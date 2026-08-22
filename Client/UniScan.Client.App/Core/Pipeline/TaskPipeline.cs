using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R3;

namespace UniScan.Client.App.Core.Pipeline;

public class TaskPipeline
{
    public BindableReactiveProperty<double> Progress { get; } = new(0.0);
    public BindableReactiveProperty<string> Status { get; } = new("Starting");

    private readonly List<ITaskStage> _stages = [];
    
    internal TaskPipeline() {}//only allow builder to call ctor

    internal void Add(ITaskStage stage) => _stages.Add(stage);

    public async Task RunAsync(ITaskContext initial)
    {
        ITaskContext currentContext = initial;
        IDisposable? subscription = null;
        
        int tasks = _stages.Sum(s => s.Count);
        int completed = 0;
        try
        {
            foreach (ITaskStage stage in _stages)
            {
                currentContext = stage.Transition(currentContext);

                subscription?.Dispose(); //make sure old is disposed otherwise we will leak subscriptions
                //todo see comment in itaskcontext
                subscription = currentContext.Status.Subscribe(s => Status.Value = s);

                foreach (var task in stage.Tasks)
                {
                    await task(currentContext);

                    completed++;
                    Progress.Value = (completed * 100) / tasks;
                }
            }
        }//todo catch exception? idk how to get this fucking exception to bubble
        finally
        {
            subscription?.Dispose();
        }
    }
}