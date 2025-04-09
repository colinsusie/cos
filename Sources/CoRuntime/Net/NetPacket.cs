// Written by Colin on 2025-02-22

using CoLib.ObjectPools;

namespace CoRuntime.Net;

/**
 * 网络消息包格式
 * - Length不包含自身
 * - 小端字节序
 * 
 * |--Length(4B)--|---RmtNotify(1B)---|---Flags(1B)---|---MessageId(2B)---|---Content(NB)---|
 */

public class NetPacket: IDisposable, ICleanable
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void Cleanup()
    {
        throw new NotImplementedException();
    }
}