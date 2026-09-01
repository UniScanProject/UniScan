namespace UniScan.Server.Core.Host;

public record ConnectionTask(
    Task Task, CancellationTokenSource TokenSource
);