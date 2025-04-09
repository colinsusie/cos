// Written by Colin on 2025-02-20

namespace CoRuntime.Services.Uid;


public sealed class UidServiceOptions
{
    /// 是否使用时间服务，不使用的话就用本地时间
    public bool UseTimeService { get; set; }
    /// UTC基础时间戳(秒)：2025-1-1 00:00:00
    public long BaseTimestamp { get; set; } = 1735660800;
    /// 节点ID占的位数，以下三个加起来必须是63位
    public short NodeIdBits { get; set; } = 12;
    /// 时间戳占的位数
    public short TimestampBits { get; set; } = 31;
    /// 自增ID占的位数
    public short IncIdBits { get; set; } = 20;
}