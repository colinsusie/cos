// Written by Colin on 2023-12-15
namespace CoLib.Message;

/// <summary>
/// 代表消息循环中的一个消息
/// </summary>
public interface IMessage: IDisposable
{
    public void Process();
}