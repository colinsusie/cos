// Written by Colin on 2024-8-24

using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;

namespace CoLib.EventLoop;

/// <summary>
/// 实现DotNetty的事件循环组
/// </summary>
public class MtEventLoopGroup: IEventLoopGroup
{
    static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromMilliseconds(100);
    static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(5);
    
    private readonly IEventLoop[] _eventLoops;
    private int _requestId;
    
    public MtEventLoopGroup(Func<IEventLoopGroup, IEventLoop> eventLoopFactory, int eventLoopCount)
    {
        _eventLoops = new IEventLoop[eventLoopCount];
        var terminationTasks = new Task[eventLoopCount];
        for (int i = 0; i < eventLoopCount; i++)
        {
            IEventLoop eventLoop;
            bool success = false;
            try
            {
                eventLoop = eventLoopFactory(this);
                success = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("failed to create a child event loop.", ex);
            }
            finally
            {
                if (!success)
                {
                    Task.WhenAll(
                            _eventLoops
                                .Take(i)
                                .Select(loop => loop.ShutdownGracefullyAsync()))
                        .Wait();
                }
            }

            _eventLoops[i] = eventLoop;
            terminationTasks[i] = eventLoop.TerminationCompletion;
        }
        TerminationCompletion = Task.WhenAll(terminationTasks);
    }
    
    #region IExecutorImpl
    
    public void Execute(IRunnable task) => GetNext().Execute(task);
    public void Execute(Action<object> action, object state) => GetNext().Execute(action, state);
    public void Execute(Action action) => GetNext().Execute(action);
    public void Execute(Action<object, object> action, object context, object state) => GetNext().Execute(action, context, state);
    
    #endregion

    #region IExecutorServiceImpl
    public Task<T> SubmitAsync<T>(Func<T> func) => GetNext().SubmitAsync(func);
    public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken) => GetNext().SubmitAsync(func, cancellationToken);
    public Task<T> SubmitAsync<T>(Func<object, T> func, object state) => GetNext().SubmitAsync(func, state);
    public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)  
        => GetNext().SubmitAsync(func, state, cancellationToken);
    public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state)
        => GetNext().SubmitAsync(func, context, state);
    public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
        => GetNext().SubmitAsync(func, context, state, cancellationToken);
    public bool IsShutdown => _eventLoops.All(eventLoop => eventLoop.IsShutdown);
    public bool IsTerminated => _eventLoops.All(eventLoop => eventLoop.IsTerminated);
    
    #endregion

    #region IScheduledExecutorServiceImpl
    
    public IScheduledTask Schedule(IRunnable action, TimeSpan delay) => GetNext().Schedule(action, delay);
    public IScheduledTask Schedule(Action action, TimeSpan delay) => GetNext().Schedule(action, delay);
    public IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay) => GetNext().Schedule(action, state, delay);
    public IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay) 
        => GetNext().Schedule(action, context, state, delay);
    public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken)
        => GetNext().ScheduleAsync(action, state, delay, cancellationToken);
    public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay)
        => GetNext().ScheduleAsync(action, state, delay);
    public Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        => GetNext().ScheduleAsync(action, delay, cancellationToken);
    public Task ScheduleAsync(Action action, TimeSpan delay)
        => GetNext().ScheduleAsync(action, delay);
    public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay)
        => GetNext().ScheduleAsync(action, context, state, delay);
    public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken)
        => GetNext().ScheduleAsync(action, context, state, delay);
    
    #endregion

    #region IEventExecutorGroupImpl
    
    public Task ShutdownGracefullyAsync()
        => ShutdownGracefullyAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);

    public Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
    {
        foreach (IEventLoop eventLoop in _eventLoops)
        {
            eventLoop.ShutdownGracefullyAsync(quietPeriod, timeout);
        }
        return TerminationCompletion;
    }
    
    public IEnumerable<IEventLoop> Items => _eventLoops;
    
    public Task RegisterAsync(IChannel channel)
        => ((IEventLoop)GetNext()).RegisterAsync(channel);

    public IEventExecutor GetNext()
    {
        int id = Interlocked.Increment(ref _requestId);
        return _eventLoops[Math.Abs(id % _eventLoops.Length)];
    }
    
    #endregion

    #region IEventLoopGroupImpl
    
    IEventLoop IEventLoopGroup.GetNext() => (IEventLoop)GetNext();
    
    IEnumerable<IEventExecutor> IEventExecutorGroup.Items => Items;

    public bool IsShuttingDown=> _eventLoops.All(eventLoop => eventLoop.IsShuttingDown);
    public Task TerminationCompletion { get; }
    
    #endregion
}