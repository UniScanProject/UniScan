using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniScan.Client.App.Core.Pipeline;

public interface ITaskStage
{
    int Count { get; }
    IEnumerable<Func<ITaskContext, Task>> Tasks { get; }

    ITaskContext Transition(ITaskContext old);
}