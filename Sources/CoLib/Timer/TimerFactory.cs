// Written by Colin on 2023-12-09

using CoLib.Logging;

namespace CoLib.Timer;

public static class TimerFactory
{
    /// <summary>
    /// 创建基于优先队列的定时调度器
    /// </summary>
    /// <returns></returns>
    public static ITimerScheduler CreatePqTimerScheduler(Logger logger)
    {
        return new PqTimerScheduler(logger);
    }

    /// <summary>
    /// 创建基于时间轮的定时调度器
    /// </summary>
    /// <returns></returns>
    public static ITimerScheduler CreateWhTimerScheduler(WhTimerOptions options, Logger logger)
    {
        return new WhTimerScheduler(options, logger);
    }
}