// Written by Colin on 2024-9-24

namespace CoRuntime.Rpc;

public class PredefineMethodId
{
    // 保留的RPC方法Id范围
    public const short MinId = 30000;
    public const short MaxId = 31000;
    
    // 心跳消息
    public const short PingPong = 30000;
}