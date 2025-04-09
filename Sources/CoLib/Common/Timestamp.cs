// Written by Colin on 2024-10-20

using System.Diagnostics.CodeAnalysis;

namespace CoLib.Common;

/// <summary>
/// 代表兼容Unix的时间戳
/// </summary>
public readonly struct Timestamp:
    IComparable,
    IComparable<Timestamp>,
    IEquatable<Timestamp>
{
    private readonly DateTime _utcTime;

    private Timestamp(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
            ThrowHelper.ThrowInvalidOperationException("utcTime must be Utc kind");
        _utcTime = utcTime;
    }

    public DateTime UtcTime => _utcTime;
    
    public DateTime LocalTime => _utcTime.ToLocalTime();

    public static Timestamp UtcNow => new(DateTime.UtcNow);

    public long ToUnixTimeSeconds() => _utcTime.Ticks / 10000000L - 62135596800L;
    
    public long ToUnixTimeMilliseconds() => _utcTime.Ticks / 10000L - 62135596800000L;

    public static Timestamp FromUnixTimeSeconds(long seconds)
    {
        if (seconds is < -62135596800L or > 253402300799L)
            ThrowHelper.ThrowArgumentOutOfRangeException($"Seconds out of range: {seconds}");
        return new Timestamp(new DateTime(seconds * 10000000L + 621355968000000000L, DateTimeKind.Utc));
    }
    
    public static Timestamp FromUnixTimeMilliseconds(long ms)
    {
        if (ms is < -62135596800000L or > 253402300799999L)
            ThrowHelper.ThrowArgumentOutOfRangeException($"Seconds out of range: {ms}");
        return new Timestamp(new DateTime(ms * 10000L + 621355968000000000L, DateTimeKind.Utc));
    }
    
    public static int Compare(Timestamp t1, Timestamp t2)
    {
        return t1._utcTime.CompareTo(t2._utcTime);
    }
    
    public int CompareTo(object? value)
    {
        if (value == null) return 1;
        if (value is not Timestamp)
        {
            ThrowHelper.ThrowArgumentException("value must be Timestamp");
        }
 
        return Compare(this, (Timestamp)value);
    }
    
    public int CompareTo(Timestamp value)
    {
        return Compare(this, value);
    }
    
    public override bool Equals([NotNullWhen(true)] object? value)
    {
        return value is Timestamp dt && this == dt;
    }

    public override int GetHashCode()
    {
        return _utcTime.GetHashCode();
    }

    public bool Equals(Timestamp value)
    {
        return this == value;
    }
    
    public static bool Equals(Timestamp t1, Timestamp t2)
    {
        return t1 == t2;
    }
    
    public static Timestamp operator +(Timestamp d, TimeSpan t)
    {
        return new Timestamp(d._utcTime + t);
    }
 
    public static Timestamp operator -(Timestamp d, TimeSpan t)
    {
        return new Timestamp(d._utcTime - t);
    }

    public static TimeSpan operator -(Timestamp d1, Timestamp d2) => d1._utcTime - d2._utcTime;
 
    public static bool operator ==(Timestamp d1, Timestamp d2) => d1._utcTime == d2._utcTime;
 
    public static bool operator !=(Timestamp d1, Timestamp d2) => !(d1 == d2);
 
    public static bool operator <(Timestamp t1, Timestamp t2) => t1._utcTime < t2._utcTime;
 
    public static bool operator <=(Timestamp t1, Timestamp t2) => t1._utcTime <= t2._utcTime;
 
    public static bool operator >(Timestamp t1, Timestamp t2) => t1._utcTime > t2._utcTime;
 
    public static bool operator >=(Timestamp t1, Timestamp t2) => t1._utcTime >= t2._utcTime;
}