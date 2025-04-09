// Written by Colin on 2025-02-12

namespace CoLib.Extensions;

/// <summary>
/// Task/ValueTask的一些扩展
/// </summary>
public static class TaskExt
{
    // / 给Task加上CancellationToken，当取消时会抛出TaskCanceledException
    public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {
        return task.WaitAsync(cancellationToken);
    }

    // / 给Task加上CancellationToken，当取消时会抛出TaskCanceledException
    public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
    {
        return task.WaitAsync(cancellationToken);
    }
}