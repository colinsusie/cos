// Written by Colin on 2023-12-15

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using CoLib.Common;
using CoLib.Extensions;
using CoLib.Logging;

namespace CoLib.Message;

/// <summary>
/// 消息循环，里面的消息队列串行执行
/// </summary>
public class MessageLoop
{
    private const int StateNew = 0;             // 初始
    private const int StateRunning = 1;         // 正在运行
    private const int StateStopping = 2;        // 正在停止
    private const int StateStopped = 3;         // 已停止
    
    // 消息调度器
    private readonly MessageScheduler _scheduler;
    private readonly MessageLoopBox _loopBox; 
    
    private readonly ChannelWriter<IMessage> _channelWriter;
    private readonly ChannelReader<IMessage> _channelReader;
    
    private readonly TaskCompletionSource _stopTsc = new ();
    private readonly Logger _logger;
    // 当前状态
    private volatile int _state = StateNew;

    public MessageLoop(Logger logger)
    {
        _logger = logger;
        _loopBox = new MessageLoopBox(this);
        _scheduler = new MessageScheduler(this);
        var channel = Channel.CreateUnbounded<IMessage>();
        _channelWriter = channel.Writer;
        _channelReader = channel.Reader;
    }

    // 是否正在运行
    public bool IsRunning => _state == StateRunning;
    // 是否已经停止
    public bool IsStopped => _state == StateStopped;

    /// <summary>
    /// 启动消息循环
    /// </summary>
    public Task StartAsync()
    {
        ChangeState(StateNew, StateRunning);
        // 确保在线程池执行
        Task.Run(Loop);
        
        return Task.CompletedTask;
    }
    
    // 循环函数
    private async ValueTask Loop()
    {
        while (await _channelReader.WaitToReadAsync())
        {
            ProcessMessages();
        }

        DoStop();
    }

    private void ProcessMessages()
    {
        Task.Factory.StartNew(static loopBox =>
        {
            var loop = ((MessageLoopBox) loopBox!).Loop;
            var reader = loop._channelReader;
            while (reader.TryRead(out var message))
            {
                loop.ProcessMessage(message);
            }
        }, _loopBox, CancellationToken.None, TaskCreationOptions.None, _scheduler);
    }

    private void ProcessMessage(IMessage message)
    {
        try
        {
            message.Process();
        }
        catch (Exception e)
        {
            _logger.Error($"message: {message}, error:{e}");
        }
        finally
        {
            message.Dispose();
        }
    }

    /// <summary>
    /// 停止消息循环
    /// </summary>
    /// <returns></returns>
    public Task StopAsync()
    {
        ChangeState(StateRunning, StateStopping);
        _channelWriter.TryComplete();
        return _stopTsc.Task;
    }

    private void DoStop()
    {
        ChangeState(StateStopping, StateStopped);
        _stopTsc.TrySetResult();
    }

    /// <summary>
    /// 向消息循环投递消息，必须保证在StateRunning状态下才能调用这个函数
    /// 这里不判断是为了节省一点性能
    /// </summary>
    /// <param name="message"></param>
    public bool Post(IMessage message)
    {
        return _channelWriter.TryWrite(message);
    }

    /// <summary>
    /// 在消息循环中执行一个有状态的Action
    /// </summary>
    public void Post<TArgs>(Action<TArgs> action, in TArgs args)
    {
        Post(ActionMessage<TArgs>.Create(action, args));
    }

    /// <summary>
    /// 在消息循环中执行一个无返回值的异步函数，并返回完成的ValueTask
    /// </summary>
    /// <param name="args">传给函数的参数</param>
    ///   函数返回后在哪里执行：<br/>
    ///   1. 如果在消息循环中调用该函数，返回后还在本消息循环中 <br/>
    ///   2. 没有则,在线程池执行<br/>
    /// <param name="func">要执行的回调函数</param>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ExecuteAsync<TArgs>(Func<TArgs, ValueTask> func, TArgs args)
    {
        var message = AsyncFuncMessage<TArgs, ValueTask>.Create(func, args);
        Post(message);
        var task = await message.ValueTask;
        await task;
    }

    /// <summary>
    /// 在消息循环中执行一个有返回值的异步函数，并返回完成的ValueTask
    /// </summary>
    /// <param name="args">传给Action的参数</param>
    ///   函数返回后在哪里执行：<br/>
    ///   1. 如果在消息循环中调用该函数，返回后还在本消息循环中 <br/>
    ///   2. 没有则,在线程池执行<br/>
    /// <param name="func">要执行的回调函数</param>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<TResult> ExecuteAsync<TArgs, TResult>(Func<TArgs, ValueTask<TResult>> func, TArgs args)
    {
        var message = AsyncFuncMessage<TArgs, ValueTask<TResult>>.Create(func, args);
        Post(message);
        var task = await message.ValueTask;
        return await task;
    }
    
    // 转换状态
    private void ChangeState(int oldState, int newState)
    {
        if (Interlocked.CompareExchange(ref _state, newState, oldState) == oldState)
            return;

        throw new StateException($"failed to change state: {newState}, oldState must be {oldState} but now is {_state}");
    }

    // 检查当前状态
    private void CheckState(int state)
    {
        if (_state != state)
        {
            throw new StateException($"current state is {_state}, the requested status is {state}");
        }
    }
}

internal class MessageLoopBox
{
    public readonly MessageLoop Loop;

    public MessageLoopBox(MessageLoop loop)
    {
        Loop = loop;
    }
}