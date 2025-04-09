// Written by Colin on 2023-12-09

using CoLib.Container;

namespace CoLib.Timer;

/// <summary>
/// 定时器组
/// </summary>
public interface ITimerGroup
{
}

public interface ITimerScheduler
{
    /// <summary>
    /// 增加一个定时器
    /// </summary>
    /// <param name="group">定时器所属的组，为空表示没有组</param>
    /// <param name="firstTime">第一次回调时间：毫秒</param>
    /// <param name="periodTime">后面的周期回调时间，如果为0表示没有周期时间：毫秒</param>
    /// <param name="timerFunc">定时器回调函数</param>
    /// <param name="dataItem">回调函数状态数据</param>
    /// <returns>返回代表定时器的Id</returns>
    public long AddTimer(ITimerGroup? group, uint firstTime, uint periodTime, Action<DataItem?> timerFunc, DataItem? dataItem);
    public long AddTimer(uint firstTime, uint periodTime, Action<DataItem?> timerFunc, DataItem? data)
    {
        return AddTimer(null, firstTime, periodTime, timerFunc, data);
    }
    public long AddTimer(uint firstTime, uint periodTime, Action<DataItem?> timerFunc)
    {
        return AddTimer(null, firstTime, periodTime, timerFunc, null);
    }

    /// <summary>
    /// 删除定时器
    /// </summary>
    /// <param name="timerId">定时器Id</param>
    /// <returns>返回是否删除成功，失败表示没有该定时器</returns>
    public bool RemoveTimer(long timerId);

    /// <summary>
    /// 删除某个定时器组的所有定时器
    /// </summary>
    /// <param name="group">定时器组</param>
    /// <returns>返回是否删除成功，失败表示找不到该定时器组</returns>
    public bool RemoveGroupTimers(ITimerGroup group);

    /// <summary>
    /// 调度定时器，需要不断的调用这个函数，驱动定时器的触发
    /// </summary>
    public void Schedule();

    // 取调试信息
    public string GetDebugInfo(bool detail);
}