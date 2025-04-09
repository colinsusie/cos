namespace CoLib.UniqueId;

/// <summary>
/// 唯一ID选项
/// </summary>
public sealed class UidOptions
{
    /// UTC基础时间戳(秒)
    public long BaseTimestamp { get; set; }
    /// 节点ID
    public short NodeId { get; set; }
    /// 节点ID占的位数，以下三个加起来必须是63位
    public short NodeIdBits { get; set; }
    /// 时间戳占的位数
    public short TimestampBits { get; set; }
    /// 自增ID占的位数
    public short IncIdBits { get; set; }
}