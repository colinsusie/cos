// Written by Colin on 2023-12-19

using CoLib.EventLoop;
using CoLib.Logging;

namespace CoRuntime.Services;

/// <summary>
/// 服务基础类
/// </summary>
public abstract class Service: RuntimeObject
{
    public readonly ServiceContext ServiceCtx;
    public short ServiceId => ServiceCtx.ServiceId;
    public string ServiceName => ServiceCtx.Options.ServiceName;
    public StEventLoop EventLoop => ServiceCtx.EventLoop;
    
    protected Service(ServiceContext serviceCtx)
    {
        ServiceCtx = serviceCtx;
    }

    public override string ToString()
    {
        return $"Service:{ServiceName}:{ServiceId}";
    }

    /// 程序启完毕，子类可以处理逻辑
    public virtual void OnAppStarted()
    {
    }
    
    /// 取一个日志器
    public Logger GetLogger(string tag)
    {
        return ServiceCtx.GetLogger(tag);
    }
    
    /// 在下一帧创建一个异步任务
    /// cancellationToken 用于通知任务提前结束
    protected void PostTask(Func<CancellationToken, Task> func)
    {
        _ = ServiceCtx.EventLoop.SubmitExAsync(static async (service, fun) =>
        {
            if (service.IsStopped)
                return;
            try
            {
                await fun(service.CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                service.Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                service.Logger.Error($"Error: {e}");
            }
        }, this, func);
    }

    /// 在下一帧创建一个异步任务
    /// cancellationToken 用于通知任务提前结束
    protected void PostTask<TArg1>(Func<TArg1, CancellationToken, Task> func, TArg1 a1)
    {
        _ = ServiceCtx.EventLoop.SubmitExAsync(static async (service, fun, a1) =>
        {
            if (service.IsStopped)
                return;
            
            try
            {
                await fun(a1, service.CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                service.Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                service.Logger.Error($"Error: {e}");
            }
        }, this, func, a1);
    }
    
    /// 在下一帧创建一个异步任务
    /// cancellationToken 用于通知任务提前结束
    protected void PostTask<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> func, TArg1 a1, TArg2 a2)
    {
        _ = ServiceCtx.EventLoop.SubmitExAsync(static async (service, fun, a1, a2) =>
        {
            if (service.IsStopped)
                return;
            
            try
            {
                await fun(a1, a2, service.CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                service.Logger.Info($"Task cancel: {e}");
            }
            catch (Exception e)
            {
                service.Logger.Error($"Error: {e}");
            }
        }, this, func, a1, a2);
    }

    /// 在下一帧执行一个函数
    protected void Post(Action action)
    {
        ServiceCtx.EventLoop.Execute(action);
    }

    /// 在下一帧执行一个函数
    protected void Post<TArg1>(Action<TArg1> action, TArg1 a1)
    {
        ServiceCtx.EventLoop.ExecuteEx(a1, action);
    }
    
    /// 在下一帧执行一个函数
    protected void Post<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 a1, TArg2 a2)
    {
        ServiceCtx.EventLoop.ExecuteEx(a1, a2, action);
    }
}