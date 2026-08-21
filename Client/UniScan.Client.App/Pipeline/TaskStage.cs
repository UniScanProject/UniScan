using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniScan.Client.App.Pipeline;

public class TaskStage<TOldContext, TContext> : ITaskStage
    where TOldContext : class, ITaskContext
    where TContext : class, ITaskContext
{
    private readonly Func<TOldContext, TContext> _transition;
    private readonly List<Func<TContext, Task>> _tasks = [];

    public int Count => _tasks.Count;

    public IEnumerable<Func<ITaskContext, Task>> Tasks
    {
        get
        {
            foreach (var task in _tasks)
                yield return ctx => task((TContext)ctx);
        }
    }

    public TaskStage(Func<TOldContext, TContext> transition)
    {
        _transition = transition;
    }

    public void Add(Func<TContext, Task> task) => _tasks.Add(task);
    public ITaskContext Transition(ITaskContext old) => old is not TOldContext ctx
                                                                    ? throw new InvalidCastException()
                                                                    : _transition(ctx);
}