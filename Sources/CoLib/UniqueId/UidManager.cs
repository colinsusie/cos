using CoLib.Logging;

namespace CoLib.UniqueId;

/// <summary>
/// 唯一ID管理器
/// </summary>
public sealed class UidManager
{
    private readonly Logger _logger;
    private readonly long _nodeIdPart;
    private readonly long _tsIncIdMask;
    private long _tsIncId;

    private long _availableNums;
    private readonly long _incNumberPerMs;
    private object _tickLock = new();
    private long _lastTickMs = Environment.TickCount64;
    
    public UidManager(Logger logger, IUidTimeProvider timeProvider, UidOptions options)
    {
        _logger = logger;
        var maxNodeId = (1 << options.NodeIdBits) - 1;
        if (options.NodeId > maxNodeId)
            throw new ArgumentException($"NodeId exceeds the maximum value of {maxNodeId}");
        
        if (options.NodeIdBits + options.TimestampBits + options.IncIdBits != 63)
            throw new ArgumentException($"The sum of the bits of each part must be equal to 63," +
                                        $"NodeIdBits:{options.NodeIdBits}, " +
                                        $"TimestampBits:{options.TimestampBits}," +
                                        $"IncIdBits:{options.IncIdBits}");
        
        var nowTimestamp = timeProvider.Timestamp;
        var rts = nowTimestamp - options.BaseTimestamp;
        if (rts < 0)
            throw new ArgumentException($"Relative timestamp less than 0, " +
                                        $"BaseTimestamp: {options.BaseTimestamp}," +
                                        $"Timestamp: {nowTimestamp}");
        
        var maxTimestamp = (1 << options.TimestampBits) - 1;
        if (rts > maxTimestamp)
            throw new ArgumentException($"Relative timestamp exceeds the maximum value of {maxTimestamp}");

        _incNumberPerMs = ((1 << options.IncIdBits) - 1) / 1000;
        _nodeIdPart = (long)options.NodeId << (options.TimestampBits + options.IncIdBits);
        _tsIncIdMask = ((long)1 << (options.TimestampBits + options.IncIdBits)) - 1;
        _tsIncId = (rts << options.IncIdBits) | 0;
        
        // 暂停一会儿并主动Tick一次，积累一些ID
        Thread.Sleep(100);
        Tick();
    }

    /// 生成一个UID
    public long GenerateUid()
    {
        while (true)
        {
            var currentNums = Interlocked.Read(ref _availableNums);
            if (currentNums <= 0)
            {
                _logger.Warn($"Uid generation too fast, sleep for a while and force tick");
                Thread.Sleep(10);
                // 强制Tick一下，生成可用的ID数量
                Tick();
                continue;
            }

            if (Interlocked.CompareExchange(ref _availableNums, currentNums - 1, currentNums) == currentNums)
                break;
        }
        
        var tsIncId = Interlocked.Increment(ref _tsIncId) & _tsIncIdMask;
        return _nodeIdPart | tsIncId;
    }

    /// 更新可用的UID数量，必须定时调用
    public void Tick()
    {
        lock( _tickLock)
        {
            var currTickMs = Environment.TickCount64;
            var lastTickMs = _lastTickMs;
            var ms = currTickMs - lastTickMs;
            if (ms < 0)
                return;
            
            _lastTickMs = currTickMs;
            Interlocked.Add(ref _availableNums, ms * _incNumberPerMs);
        }
    }
}