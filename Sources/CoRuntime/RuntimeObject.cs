// Written by Colin on 2025-02-09

using CoLib.Logging;

namespace CoRuntime;

public enum RuntimeObjectStatus
{
    Init,
    Starting,
    Started,
    Stopping,
    Stopped,
}

/// <summary>
/// 运行时对象
/// </summary>
public class RuntimeObject
{
    protected CancellationTokenSource StopCts = null!;
    protected CancellationToken CancellationToken => StopCts.Token;
    public readonly Logger Logger;
    public volatile RuntimeObjectStatus Status = RuntimeObjectStatus.Init;
    public bool IsStopping => Status == RuntimeObjectStatus.Stopping;
    public bool IsStarted => Status == RuntimeObjectStatus.Started;
    public bool IsStopped => Status == RuntimeObjectStatus.Stopped;
    public bool IsStoppingOrStopped => Status >= RuntimeObjectStatus.Stopping;

    protected RuntimeObject()
    {
        Logger = RuntimeEnv.LogMgr.GetLogger(GetType().Name);
    }

    private void SetStatus(RuntimeObjectStatus status)
    {
        var oldStatus = Status;
        Status = status;
        Logger.Info($"{oldStatus} => {status}");
    }
    
    /// 启动服务
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        SetStatus(RuntimeObjectStatus.Starting);
        StopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await DoStartAsync(StopCts.Token);
        // 可能已经停止了
        if (!IsStoppingOrStopped)
            SetStatus(RuntimeObjectStatus.Started);
    }

    /// 停止服务
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        SetStatus(RuntimeObjectStatus.Stopping);
        try
        {
            // ReSharper disable once MethodHasAsyncOverload
            StopCts.Cancel();   
        }
        catch (Exception e)
        {
            Logger.Error($"StopCts.Cancel error: {e}");
        }
        
        try
        {
            await DoStopAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Logger.Error($"DoStopAsync error:{e}");
        }
        finally
        {
            SetStatus(RuntimeObjectStatus.Stopped);
        }
    }
    
    // 子类处理
    protected virtual Task DoStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // 子类处理
    protected virtual Task DoStopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    /// 立即执行一个异步任务
    protected void ExecTask(Func<CancellationToken, Task> func)
    {
        _ = DoExecTask();
        return;

        async Task DoExecTask()
        {
            await Task.Yield();
            try
            {
                await func(CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                Logger.Error($"Error: {e}");
            }
        }
    }
    
    /// 立即执行一个异步任务
    protected void ExecTask<TArg1>(Func<TArg1, CancellationToken, Task> func, TArg1 a1)
    {
        _ = DoExecTask();
        return;

        async Task DoExecTask()
        {
            await Task.Yield();
            try
            {
                await func(a1, CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                Logger.Error($"Error: {e}");
            }
        }
    }
    
    /// 立即执行一个异步任务
    protected void ExecTask<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func, TArg1 a1, TArg2 a2)
    {
        _ = DoExecTask();
        return;

        async Task DoExecTask()
        {
            await Task.Yield();
            try
            {
                await func(a1, a2, CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                Logger.Error($"Error: {e}");
            }
        }
    }
}