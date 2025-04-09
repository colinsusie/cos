namespace CoLib.UniqueId;

/// <summary>
/// 唯一ID时间提供者
/// </summary>
public interface IUidTimeProvider
{
    /// 取当前的UTC时间戳(秒)
    long Timestamp { get; }
}