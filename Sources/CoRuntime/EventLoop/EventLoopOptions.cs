namespace CoRuntime.EventLoop;

/// <summary>
/// 事件循环选项
/// </summary>
public class EventLoopOptions
{
    // 事件循环数
    public int Count { get; set; } = Environment.ProcessorCount;
    // 事件循环分组 key=分组名, int 这一组的事件循环数
    public Dictionary<string, int>? Groups { get; set; }
}