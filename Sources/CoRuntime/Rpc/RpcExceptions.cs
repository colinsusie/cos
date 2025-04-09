// Written by Colin on 2024-8-24

namespace CoRuntime.Rpc;

/// <summary>
/// Rpc异常
/// </summary>
public class RpcException: Exception
{
    public RpcException(string? message): base(message)
    {
    }
}