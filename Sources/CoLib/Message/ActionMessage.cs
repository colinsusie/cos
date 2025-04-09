// Written by Colin on ${2023}-12-16

using CoLib.ObjectPools;

namespace CoLib.Message;

/// <summary>
/// 执行带状态的Action的消息
/// </summary>
internal class ActionMessage<TArgs>: IMessage, ICleanable
{
    private Action<TArgs>? _action;
    private TArgs _args = default!;
    
    [ThreadStatic] private static StObjectPool<ActionMessage<TArgs>>? _pool;
    private static StObjectPool<ActionMessage<TArgs>> Pool => _pool ??= new (256, () => new ());
    
    public static ActionMessage<TArgs> Create(Action<TArgs> action, in TArgs args)
    {
        var message = Pool.Get();
        message.Initialize(action, args);
        return message;
    }

    private ActionMessage()
    {
    }

    private void Initialize(Action<TArgs> action, in TArgs args)
    {
        _action = action;
        _args = args;
    }
    
    public void Cleanup()
    {
        _action = null;
        _args = default!;
    }
    
    public void Dispose()
    {
        Pool.Return(this);
    }

    public void Process()
    {
        _action?.Invoke(_args);
    }
}