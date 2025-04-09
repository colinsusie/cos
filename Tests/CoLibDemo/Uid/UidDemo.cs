// Written by Colin on 2024-1-27

using CoLib.Logging;
using CoLib.UniqueId;

namespace CoLibDemo.Uid;

public class TimeProvider : IUidTimeProvider
{
    public long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

public class UidDemo
{
    public static void Start()
    {
        var logMgr = new LogManagerBuilder()
            .AddOutputConsole()
            .Build();

        var logger = logMgr.GetLogger("UidDemo");
        var uidMgr = new UidManager(logMgr.GetLogger(nameof(UidManager)), new TimeProvider(), new UidOptions()
        {
            BaseTimestamp = (new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)).ToUnixTimeSeconds(),
            NodeId = 1,
            IncIdBits = 20,
            TimestampBits = 31,
            NodeIdBits = 12,
        });

        Task.Run(() =>
        {
            while (true)
            {
                uidMgr.Tick();
                Thread.Sleep(100);
            }
        });
        
        Thread.Sleep(100);
        for (var i = 0; i <= 2000000; ++i)
        {
            var uid = uidMgr.GenerateUid();
            if (i % 100000 == 0)
                logger.Info($"{uid}, " +
                            $"NodeIdPart:{uid >> 51}, " +
                            $"TimestampPart:{(uid >> 20) & ((1L << 31) - 1)}, " +
                            $"IncIdPart:{uid & ((1L << 20) -1)}");
        }
        
        logMgr.Stop();
    }
}