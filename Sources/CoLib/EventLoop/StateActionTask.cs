// Written by Colin on 2024-7-26

using CoLib.ObjectPools;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

internal sealed class StateActionTask : IRunnable, ICleanable, IDisposable
{
    private Action<object>? _action;
    private object? _state;

    [ThreadStatic] private static StObjectPool<StateActionTask>? _pool;
    private static StObjectPool<StateActionTask> Pool => _pool ??= new(128, () => new());

    public static StateActionTask Create(Action<object> action, object? state)
    {
        var message = Pool.Get();
        message.Initialize(action, state);
        return message;
    }

    private StateActionTask()
    {
    }

    private void Initialize(Action<object> action, object? state)
    {
        _action = action;
        _state = state;
    }

    void ICleanable.Cleanup()
    {
        _action = null;
        _state = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    public void Run()
    {
        _action?.Invoke(_state!);
    }
}