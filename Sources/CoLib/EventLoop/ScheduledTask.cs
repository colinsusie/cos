// Written by Colin on 2024-7-27

using System.Runtime.CompilerServices;
using DotNetty.Common;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

public abstract class ScheduledTask: IScheduledRunnable
{
    public PreciseTimeSpan Deadline { get; protected set; }
    public Task Completion => Task.CompletedTask;
    
    public bool Cancel()
    {
        return false;
    }

    public TaskAwaiter GetAwaiter()
    {
        return Task.CompletedTask.GetAwaiter();
    }
    
    public void Run()
    {
        Execute();
    }

    public int CompareTo(IScheduledRunnable? other)
    {
        return other == null ? 1 : Deadline.CompareTo(other.Deadline); 
    }
    
    protected abstract void Execute();
}