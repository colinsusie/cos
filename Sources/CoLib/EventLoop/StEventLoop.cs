// Written by Colin on 2024-7-26

using System.Collections.Concurrent;
using CoLib.Container;
using CoLib.Logging;
using DotNetty.Common;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace CoLib.EventLoop;

/// <summary>
/// 实现DotNetty的事件循环
/// TODO：存在问题：
///     1. ShutdownGracefullyAsync后如果在事件处理里一直Execute消息会关不掉。
/// </summary>
public sealed class StEventLoop : IEventLoop
{
    private const int StNotStarted = 1;
    private const int StStarted = 2;
    private const int StShuttingDown = 3;
    private const int StShutdown = 4;
    private const int StTerminated = 5;
    private const string DefaultThreadName = "StEventLoop";
    private static readonly TimeSpan DefaultBreakoutInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(5);

    private readonly Logger _logger;
    private readonly ConcurrentQueue<IRunnable> _taskQueue = new();
    private readonly PriorityQueue<IScheduledRunnable> _scheduledTaskQueue = new();
    private readonly Thread _thread;
    private readonly TaskScheduler _scheduler;
    private volatile int _state = StNotStarted;
    private readonly PreciseTimeSpan _preciseBreakoutInterval;
    private PreciseTimeSpan _lastExecutionTime;
    private readonly TaskCompletionSource _terminationCompletionSource;
    private PreciseTimeSpan _gracefulShutdownStartTime;
    private PreciseTimeSpan _gracefulShutdownQuietPeriod;
    private PreciseTimeSpan _gracefulShutdownTimeout;
    private long _progress;
    
    record struct GeneralArgs<T1>(T1 A1);
    record struct GeneralArgs<T1, T2>(T1 A1, T2 A2);
    record struct GeneralArgs<T1, T2, T3>(T1 A1, T2 A2, T3 A3);
    record struct GeneralArgs<T1, T2, T3, T4>(T1 A1, T2 A2, T3 A3, T4 A4);
    record struct GeneralArgs<T1, T2, T3, T4, T5>(T1 A1, T2 A2, T3 A3, T4 A4, T5 A5);
    record struct GeneralArgs<T1, T2, T3, T4, T5, T6>(T1 A1, T2 A2, T3 A3, T4 A4, T5 A5, T6 A6);

    public StEventLoop(Logger logger)
    {
        _logger = logger;
        _terminationCompletionSource = new TaskCompletionSource();
        _preciseBreakoutInterval = PreciseTimeSpan.FromTimeSpan(DefaultBreakoutInterval);
        _scheduler = new StEventLoopTaskScheduler(this);
        _thread = new Thread(Loop);
        _thread.Name = DefaultThreadName;
        _thread.IsBackground = true;
        _thread.Start();
    }
    
    // 处理过的任务数
    public long Progress => Volatile.Read(ref _progress);
    // 队列积压长度
    public int BacklogLength => _taskQueue.Count;

    private void Loop()
    {
        Task.Factory.StartNew(
            () =>
            {
                try
                {
                    Interlocked.CompareExchange(ref _state, StStarted, StNotStarted);
                    while (!ConfirmShutdown())
                    {
                        if (!RunAllTasks(_preciseBreakoutInterval))
                        {
                            Thread.Sleep(1);
                        }
                    }

                    CleanupAndTerminate(true);
                }
                catch (Exception ex)
                {
                    _logger.Error($"{_thread.Name}: execution loop failed, e:{ex}");
                    _state = StTerminated;
                    _terminationCompletionSource.TrySetException(ex);
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            _scheduler);
    }

    private bool ConfirmShutdown()
    {
        if (!IsShuttingDown)
        {
            return false;
        }

        CancelScheduledTasks();

        if (_gracefulShutdownStartTime == PreciseTimeSpan.Zero)
        {
            _gracefulShutdownStartTime = PreciseTimeSpan.FromStart;
        }

        if (RunAllTasks())
        {
            if (IsShutdown)
            {
                // Executor shut down - no new tasks anymore.
                return true;
            }

            return false;
        }

        PreciseTimeSpan nanoTime = PreciseTimeSpan.FromStart;

        if (IsShutdown || (nanoTime - _gracefulShutdownStartTime > _gracefulShutdownTimeout))
        {
            return true;
        }

        if (nanoTime - _lastExecutionTime <= _gracefulShutdownQuietPeriod)
        {
            // Check if any tasks were added to the queue every 100ms.
            // TODO: Change the behavior of takeTask() so that it returns on timeout.
            // todo: ???
            Thread.Sleep(10);

            return false;
        }

        // No tasks were added for last quiet period - hopefully safe to shut down.
        // (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
        return true;
    }

    private void CleanupAndTerminate(bool success)
    {
        while (true)
        {
            int oldState = _state;
            if ((oldState >= StShuttingDown) ||
                (Interlocked.CompareExchange(ref _state, StShuttingDown, oldState) == oldState))
            {
                break;
            }
        }

        // Check if confirmShutdown() was called at the end of the loop.
        if (success && (_gracefulShutdownStartTime == PreciseTimeSpan.Zero))
        {
            _logger.Error($"Buggy {nameof(IEventExecutor)} implementation; {nameof(StEventLoop)}.ConfirmShutdown() " +
                          $"must be called before run() implementation terminates.");
        }

        try
        {
            // Run all remaining tasks and shutdown hooks.
            while (true)
            {
                if (ConfirmShutdown())
                {
                    break;
                }
            }
        }
        finally
        {
            try
            {
                Cleanup();
            }
            finally
            {
                Interlocked.Exchange(ref _state, StTerminated);
                if (!_taskQueue.IsEmpty)
                {
                    _logger.Warn($"An event executor terminated with non-empty task queue ({_taskQueue.Count})");
                }

                //firstRun = true;
                _terminationCompletionSource.Complete();
            }
        }
    }

    private void Cleanup()
    {
        // NOOP
    }

    // 返回true表示有执行过任务
    private bool RunAllTasks()
    {
        FetchFromScheduledTaskQueue();
        IRunnable? task = PollTask();
        if (task == null)
        {
            return false;
        }

        while (true)
        {
            SafeExecute(task);
            task = PollTask();
            if (task == null)
            {
                _lastExecutionTime = PreciseTimeSpan.FromStart;
                return true;
            }
        }
    }

    // 返回true表示有执行过任务
    private bool RunAllTasks(PreciseTimeSpan timeout)
    {
        FetchFromScheduledTaskQueue();
        IRunnable? task = PollTask();
        if (task == null)
        {
            return false;
        }

        PreciseTimeSpan deadline = PreciseTimeSpan.Deadline(timeout);
        long runTasks = 0;
        PreciseTimeSpan executionTime;
        while (true)
        {
            SafeExecute(task);

            runTasks++;

            // Check timeout every 64 tasks because nanoTime() is relatively expensive.
            // XXX: Hard-coded value - will make it configurable if it is really a problem.
            if ((runTasks & 0x3F) == 0)
            {
                executionTime = PreciseTimeSpan.FromStart;
                if (executionTime >= deadline)
                {
                    break;
                }
            }
            
            task = PollTask();
            if (task == null)
            {
                executionTime = PreciseTimeSpan.FromStart;
                break;
            }
        }

        _lastExecutionTime = executionTime;
        return true;
    }

    private void FetchFromScheduledTaskQueue()
    {
        if (_scheduledTaskQueue.Count == 0)
            return;
        
        PreciseTimeSpan nanoTime = PreciseTimeSpan.FromStart;
        IScheduledRunnable? scheduledTask = PollScheduledTask(nanoTime);
        while (scheduledTask != null)
        {
            _taskQueue.Enqueue(scheduledTask);
            scheduledTask = PollScheduledTask(nanoTime);
        }
    }

    private IRunnable? PollTask()
    {
        return _taskQueue.TryDequeue(out var task) ? task : null;
    }
    
    private void SafeExecute(IRunnable task)
    {
        try
        {
            Volatile.Write(ref _progress, _progress + 1);
            task.Run();
        }
        catch (Exception ex)
        {
            _logger.Warn($"A task raised an exception. Task: {task}, e:{ex}");
        }
        finally
        {
            if (task is IDisposable disposer)
            {
                disposer.Dispose();
            }
        }
    }

    #region IExecutorImpl

    public void Execute(IRunnable task)
    {
        _taskQueue.Enqueue(task);
    }
    
    public void Execute(Action action)
    {
        Execute(ActionTask.Create(action));
    }

    public void Execute(Action<object> action, object state)
    {
        Execute(StateActionTask.Create(action, state));
    }

    public void Execute(Action<object, object> action, object context, object state)
    {
        Execute(StateActionWithContextTask.Create(action, state, context));
    }

    #endregion

    #region IExecutorServiceImpl

    public Task<T> SubmitAsync<T>(Func<T> func)
    {
        return SubmitAsync(func, CancellationToken.None);
    }

    public Task<T> SubmitAsync<T>(Func<object, T> func, object state)
    {
        return SubmitAsync(func, state, CancellationToken.None);
    }
    
    public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state)
    {
        return SubmitAsync(func, context, state, CancellationToken.None);
    }
    
    public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
    {
        var node = FuncTask<T>.Create(func, cancellationToken);
        Execute(node);
        return node.Completion;
    }

    public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
    {
        var node = StateFuncTask<T>.Create(func, state, cancellationToken);
        Execute(node);
        return node.Completion;
    }
    
    public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state,
        CancellationToken cancellationToken)
    {
        var node = StateFuncWithContextTask<T>.Create(func, context, state, cancellationToken);
        Execute(node);
        return node.Completion;
    }

    public bool IsShutdown => _state >= StShutdown;
    public bool IsTerminated => _state == StTerminated;

    #endregion

    #region IScheduledExecutorServiceImpl

    public IScheduledTask Schedule(IRunnable action, TimeSpan delay)
    {
        return Schedule(RunnableScheduledTask.Create(action, PreciseTimeSpan.Deadline(delay)));
    }

    public IScheduledTask Schedule(Action action, TimeSpan delay)
    {
        return Schedule(ActionScheduledTask.Create(action, PreciseTimeSpan.Deadline(delay)));
    }

    public IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
    {
        return Schedule(StateActionScheduledTask.Create(action, state, PreciseTimeSpan.Deadline(delay)));
    }

    public IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay)
    {
        return Schedule(
            StateActionWithContextScheduledTask.Create(action, context, state, PreciseTimeSpan.Deadline(delay)));
    }
    
    public Task ScheduleAsync(Action action, TimeSpan delay)
    {
        return ScheduleAsync(action, delay, CancellationToken.None);
    }
    
    public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay)
    {
        return ScheduleAsync(action, state, delay, CancellationToken.None);
    }
    
    public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay)
    {
        return ScheduleAsync(action, context, state, delay, CancellationToken.None);
    }
    
    public Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return TaskEx.Cancelled;
        }

        return Schedule(new ActionScheduledAsyncTask(this, action, PreciseTimeSpan.Deadline(delay), cancellationToken))
            .Completion;
    }

    public Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return TaskEx.Cancelled;
        }

        return Schedule(new StateActionScheduledAsyncTask(this, action, state, PreciseTimeSpan.Deadline(delay),
            cancellationToken)).Completion;
    }
    
    public Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return TaskEx.Cancelled;
        }

        return Schedule(new StateActionWithContextScheduledAsyncTask(this, action, context, state,
            PreciseTimeSpan.Deadline(delay), cancellationToken)).Completion;
    }

    private IScheduledRunnable Schedule(IScheduledRunnable task)
    {
        if (InEventLoop)
        {
            _scheduledTaskQueue.Enqueue(task);
        }
        else
        {
            Execute((e, t) => ((StEventLoop) e)._scheduledTaskQueue.Enqueue((IScheduledRunnable) t), this, task);
        }

        return task;
    }

    internal void RemoveScheduled(IScheduledRunnable task)
    {
        if (InEventLoop)
        {
            _scheduledTaskQueue.Remove(task);
        }
        else
        {
            Execute((e, t) => ((StEventLoop) e)._scheduledTaskQueue.Remove((IScheduledRunnable) t), this, task);
        }
    }

    private static bool IsNullOrEmpty<T>(PriorityQueue<T>? taskQueue)
        where T : class
    {
        return taskQueue == null || taskQueue.Count == 0;
    }

    private void CancelScheduledTasks()
    {
        PriorityQueue<IScheduledRunnable> scheduledTaskQueue = _scheduledTaskQueue;
        if (IsNullOrEmpty(scheduledTaskQueue))
        {
            return;
        }

        IScheduledRunnable[] tasks = scheduledTaskQueue.ToArray();
        foreach (IScheduledRunnable t in tasks)
        {
            t.Cancel();
        }

        _scheduledTaskQueue.Clear();
    }

    private IScheduledRunnable? PollScheduledTask(PreciseTimeSpan nanoTime)
    {
        IScheduledRunnable scheduledTask = _scheduledTaskQueue.Peek();
        if (scheduledTask == null)
        {
            return null;
        }

        if (scheduledTask.Deadline <= nanoTime)
        {
            _scheduledTaskQueue.Dequeue();
            return scheduledTask;
        }

        return null;
    }

    #endregion

    #region IEventExecutorGroupImpl

    public Task ShutdownGracefullyAsync()
    {
        return ShutdownGracefullyAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);
    }

    public Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
    {
        if (IsShuttingDown)
        {
            return TerminationCompletion;
        }

        bool inEventLoop = InEventLoop;
        while (true)
        {
            if (IsShuttingDown)
            {
                return TerminationCompletion;
            }

            int newState;
            var oldState = _state;
            if (inEventLoop)
            {
                newState = StShuttingDown;
            }
            else
            {
                switch (oldState)
                {
                    case StNotStarted:
                    case StStarted:
                        newState = StShuttingDown;
                        break;
                    default:
                        newState = oldState;
                        break;
                }
            }

            if (Interlocked.CompareExchange(ref _state, newState, oldState) == oldState)
            {
                break;
            }
        }

        _gracefulShutdownQuietPeriod = PreciseTimeSpan.FromTimeSpan(quietPeriod);
        _gracefulShutdownTimeout = PreciseTimeSpan.FromTimeSpan(timeout);

        // todo: revisit
        //if (oldState == ST_NOT_STARTED)
        //{
        //    scheduleExecution();
        //}

        return TerminationCompletion;
    }

    IEventExecutor IEventExecutorGroup.GetNext() => GetNext();
    IEnumerable<IEventExecutor> IEventExecutorGroup.Items => Items;

    public bool IsShuttingDown => _state >= StShuttingDown;
    public Task TerminationCompletion => _terminationCompletionSource.Task;

    #endregion

    #region IEventExecutorImpl

    public bool InEventLoop => IsInEventLoop(Thread.CurrentThread);

    public bool IsInEventLoop(Thread t) => _thread == t;

    IEventExecutorGroup? IEventExecutor.Parent => Parent;

    #endregion

    #region IEventLoopGroupImpl

    public IEventLoop GetNext()
    {
        return this;
    }

    public IEnumerable<IEventLoop> Items => new[] {this};

    public Task RegisterAsync(IChannel channel)
    {
        return channel.Unsafe.RegisterAsync(this);
    }

    #endregion

    #region IEventLoopImpl

    public IEventLoopGroup? Parent => null;

    #endregion
    
    #region ExecuteExImpl
    
    public void ExecuteEx<T1>(T1 a1, Action<T1> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1>(a1));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1>>();
            dataItem.Dispose();
            
            ((Action<T1>) state)(args.A1);
        }, data, action);
    }
    
    public void ExecuteEx<T1, T2>(T1 a1, T2 a2, Action<T1, T2> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1, T2>(a1, a2));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1, T2>>();
            dataItem.Dispose();
            
            ((Action<T1, T2>) state)(args.A1, args.A2);
        }, data, action);
    }
    
    public void ExecuteEx<T1, T2, T3>(T1 a1, T2 a2, T3 a3, Action<T1, T2,T3> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1, T2, T3>(a1, a2, a3));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1, T2, T3>>();
            dataItem.Dispose();
            
            ((Action<T1, T2, T3>) state)(args.A1, args.A2, args.A3);
        }, data, action);
    }
    
    public void ExecuteEx<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4, Action<T1, T2, T3, T4> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1, T2, T3, T4>(a1, a2, a3, a4));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1, T2, T3, T4>>();
            dataItem.Dispose();
            
            ((Action<T1, T2, T3, T4>) state)(args.A1, args.A2, args.A3, args.A4);
        }, data, action);
    }
    
    public void ExecuteEx<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, Action<T1, T2, T3, T4, T5> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1, T2, T3, T4, T5>(a1, a2, a3, a4, a5));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1, T2, T3, T4, T5>>();
            dataItem.Dispose();
            
            ((Action<T1, T2, T3, T4, T5>) state)(args.A1, args.A2, args.A3, args.A4, args.A5);
        }, data, action);
    }
    
    public void ExecuteEx<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, 
        Action<T1, T2, T3, T4, T5, T6> action)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<T1, T2, T3, T4, T5, T6>(a1, a2, a3, a4, a5, a6));
        Execute(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<T1, T2, T3, T4, T5, T6>>();
            dataItem.Dispose();
            
            ((Action<T1, T2, T3, T4, T5, T6>) state)(args.A1, args.A2, args.A3, args.A4, args.A5, args.A6);
        }, data, action);
    }
    
    #endregion
    
    #region SubmitExAsyncImpl
    
    public Task<T> SubmitExAsync<TArg1, T>(Func<TArg1, T> func, TArg1 a1)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1>(a1));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, T>)state)(args.A1);
        }, data, func);
    }
    
    public Task<T> SubmitExAsync<TArg1, TArg2, T>(Func<TArg1, TArg2, T> func, TArg1 a1, TArg2 a2)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1, TArg2>(a1, a2));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1, TArg2>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, TArg2, T>)state)(args.A1, args.A2);
        }, data, func);
    }
    
    public Task<T> SubmitExAsync<TArg1, TArg2, TArg3, T>(Func<TArg1, TArg2, TArg3, T> func, TArg1 a1, TArg2 a2, TArg3 a3)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1, TArg2, TArg3>(a1, a2, a3));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1, TArg2, TArg3>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, TArg2, TArg3, T>)state)(args.A1, args.A2, args.A3);
        }, data, func);
    }
    
    public Task<T> SubmitExAsync<TArg1, TArg2, TArg3, TArg4, T>(Func<TArg1, TArg2, TArg3, TArg4, T> func, TArg1 a1, TArg2 a2, TArg3 a3, TArg4 a4)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1, TArg2, TArg3, TArg4>(a1, a2, a3, a4));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1, TArg2, TArg3, TArg4>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, TArg2, TArg3, TArg4, T>)state)(args.A1, args.A2, args.A3, args.A4);
        }, data, func);
    }
    
    public Task<T> SubmitExAsync<TArg1, TArg2, TArg3, TArg4, TArg5, T>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, T> func, TArg1 a1, TArg2 a2, TArg3 a3, TArg4 a4, TArg5 a5)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1, TArg2, TArg3, TArg4, TArg5>(a1, a2, a3, a4, a5));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1, TArg2, TArg3, TArg4, TArg5>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, TArg2, TArg3, TArg4, TArg5, T>)state)(args.A1, args.A2, args.A3, args.A4, args.A5);
        }, data, func);
    }
    
    public Task<T> SubmitExAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, T> func, TArg1 a1, TArg2 a2, TArg3 a3, TArg4 a4, TArg5 a5, TArg6 a6)
    {
        var data = DataItem.Create();
        data.Set(new GeneralArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(a1, a2, a3, a4, a5, a6));
        return SubmitAsync<T>(static (ctx, state) =>
        {
            var dataItem = (DataItem)ctx;
            var args = dataItem.Get<GeneralArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>>();
            dataItem.Dispose();
            
            return ((Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T>)state)(args.A1, args.A2, args.A3, args.A4, args.A5, args.A6);
        }, data, func);
    }
    #endregion
}