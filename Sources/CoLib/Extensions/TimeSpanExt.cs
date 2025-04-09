// Written by Colin on 2023-11-19

using System.Diagnostics;

namespace CoLib.Extensions;

/// <summary>
/// 对TimeSpan的扩展
/// </summary>
public static class TimeSpanExt
{
    // 起始时间点
    private static readonly long StartTime = Stopwatch.GetTimestamp();
    private static readonly double TickFrequency = (double) TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    /// <summary>
    /// 从起始时间点到现在的间隔
    /// </summary>
    public static TimeSpan FromStart()
    {
        // 起始时间点到现在的ticks
        var ticks = Stopwatch.GetTimestamp() - StartTime;
        // ticks / Stopwatch.Frequency = seconds。
        // seconds * TimeSpan.TicksPerSecond = TimeSpan.Ticks
        return TimeSpan.FromTicks((long) (ticks * TickFrequency));
    }

    /// <summary>
    /// 从起始时间点到现在，再加上timeSpan的间隔
    /// </summary>
    public static TimeSpan FromStart(TimeSpan timeSpan)
    {
        return FromStart() + timeSpan;
    }

    /// <summary>
    /// 从起始时间点到现在，再加上millisecond毫秒的间隔
    /// </summary>
    public static TimeSpan FromStart(double millisecond)
    {
        return FromStart() + TimeSpan.FromMilliseconds(millisecond);
    }
}