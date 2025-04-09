// Written by Colin on 2024-7-27

using CoLib.ObjectPools;
using DotNetty.Common.Concurrency;

namespace CoLib.EventLoop;

internal sealed class StateActionWithContextTask : IRunnable, ICleanable, IDisposable
{
    private Action<object, object>? _action;
    private object? _state;
    private object? _context;

    [ThreadStatic] private static StObjectPool<StateActionWithContextTask>? _pool;
    private static StObjectPool<StateActionWithContextTask> Pool => _pool ??= new(128, () => new());

    public static StateActionWithContextTask Create(Action<object, object> action, object state, object context)
    {
        var message = Pool.Get();
        message.Initialize(action, state, context);
        return message;
    }

    private StateActionWithContextTask()
    {
    }

    private void Initialize(Action<object, object> action, object state, object context)
    {
        _action = action;
        _state = state;
        _context = context;
    }

    void ICleanable.Cleanup()
    {
        _action = null;
        _state = null;
        _context = null;
    }

    public void Dispose()
    {
        Pool.Return(this);
    }

    public void Run()
    {
        _action?.Invoke(_context!, _state!);
    }
}