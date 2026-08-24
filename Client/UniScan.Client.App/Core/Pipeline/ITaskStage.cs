using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniScan.Client.App.Core.Pipeline;

public interface ITaskStage
{
    int Count { get; }
    IEnumerable<Func<ITaskContext, CancellationToken, Task>> Tasks { get; }

    ITaskContext Transition(ITaskContext old);
}