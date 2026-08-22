using System;
using System.Threading.Tasks;

namespace UniScan.Client.App.Core.Pipeline;

public class TaskPipelineBuilder<TContext>
    where TContext : class, ITaskContext
{
    private readonly TaskPipeline _pipeline;
    private readonly TaskStage<ITaskContext, TContext> _stage;
    
    public TaskPipelineBuilder()
    {
        _pipeline = new TaskPipeline();
        _stage = new TaskStage<ITaskContext, TContext>(ctx => (TContext)ctx);
        
        _pipeline.Add(_stage);
    }

    public TaskPipelineBuilder(TaskPipeline pipeline, TaskStage<ITaskContext, TContext> stage)
    {
        _pipeline = pipeline;
        _stage = stage;
        
        _pipeline.Add(_stage);
    }

    public TaskPipelineBuilder<TContext> ThenRun(Func<TContext, Task> task)
    {
        _stage.Add(task);
        return this;
    }

    //idk better word than transition im too sleepy
    public TaskPipelineBuilder<TNewContext> ThenTransitionTo<TNewContext>(Func<TContext, TNewContext> transition)
        where TNewContext : class, ITaskContext
    => new(_pipeline, new TaskStage<ITaskContext, TNewContext>(ctx => transition((TContext)ctx)));
    
    public TaskPipelineBuilder<TNewContext> ThenTransitionTo<TNewContext>()
        where TNewContext : class, ITaskContext<TNewContext, TContext>
        => new(_pipeline, new TaskStage<ITaskContext, TNewContext>(ctx => TNewContext.TransitionFrom((TContext)ctx)));
    
    public TaskPipeline Build() => _pipeline;
}