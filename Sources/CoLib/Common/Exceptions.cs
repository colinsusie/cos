// Written by Colin on 2023-11-24

namespace CoLib.Common;

public class SizeOutOfRangeException : Exception
{
    public SizeOutOfRangeException(string? message) : base(message)
    {
    }
}

// 选项验证异步
public class OptionsException : Exception
{
    public OptionsException(string? message) : base(message)
    {
    }
}

// 状态异步
public class StateException : Exception
{
    public StateException(string? message) : base(message)
    {
    }
}

// 重复释放
public class DuplicateDestroyException : Exception
{
    public DuplicateDestroyException(string? message) : base(message)
    {
    }
}