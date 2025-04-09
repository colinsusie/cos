// Written by Colin on 2023-11-02

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CoLib.Logging;

[InterpolatedStringHandler]
public ref struct DebugInterpolatedStringHandler
{
    private LogInterpolatedStringHandler _handler;
    public readonly bool IsEnabled;
    
    public DebugInterpolatedStringHandler(int literalLength, int formattedCount, Logger logger, out bool isEnabled,
        [CallerFilePath] string filePath = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
    {
        if (!logger.LogMgr.CheckLevel(LogLevel.Debug))
        {
            _handler = default;
            isEnabled = false;
            IsEnabled = isEnabled;
            return;
        }
        
        isEnabled = true;
        IsEnabled = isEnabled;
        _handler = new LogInterpolatedStringHandler(literalLength, formattedCount, "[D]", logger.Tag, filePath, method, line);
    }
    
    public string ToStringAndClear()
    {
        return _handler.ToStringAndClear();
    }

    public void AppendLiteral(string value)
    {
        _handler.AppendLiteral(value);
    }

    public void AppendFormatted(object? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        _handler.AppendFormatted(value, format);
    }

    public void AppendFormatted<T>(T value, int alignment)
    {
        _handler.AppendFormatted(value, alignment);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }
}

[InterpolatedStringHandler]
public ref struct InfoInterpolatedStringHandler
{
    private LogInterpolatedStringHandler _handler;
    public readonly bool IsEnabled;
    
    public InfoInterpolatedStringHandler(int literalLength, int formattedCount, Logger logger, out bool isEnabled,
        [CallerFilePath] string filePath = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
    {
        if (!logger.LogMgr.CheckLevel(LogLevel.Info))
        {
            _handler = default;
            isEnabled = false;
            IsEnabled = isEnabled;
            return;
        }

        isEnabled = true;
        IsEnabled = isEnabled;
        _handler = new LogInterpolatedStringHandler(literalLength, formattedCount, "[I]", logger.Tag, filePath, method, line);
    }
    
    public string ToStringAndClear()
    {
        return _handler.ToStringAndClear();
    }

    public void AppendLiteral(string value)
    {
        _handler.AppendLiteral(value);
    }

    public void AppendFormatted(object? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        _handler.AppendFormatted(value, format);
    }

    public void AppendFormatted<T>(T value, int alignment)
    {
        _handler.AppendFormatted(value, alignment);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }
}

[InterpolatedStringHandler]
public ref struct WarnInterpolatedStringHandler
{
    private LogInterpolatedStringHandler _handler;
    public readonly bool IsEnabled;
    
    public WarnInterpolatedStringHandler(int literalLength, int formattedCount, Logger logger, out bool isEnabled,
        [CallerFilePath] string filePath = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
    {
        if (!logger.LogMgr.CheckLevel(LogLevel.Warn))
        {
            _handler = default;
            isEnabled = false;
            IsEnabled = isEnabled;
            return;
        }

        isEnabled = true;
        IsEnabled = isEnabled;
        _handler = new LogInterpolatedStringHandler(literalLength, formattedCount, "[W]", logger.Tag, filePath, method, line);
    }
    
    public string ToStringAndClear()
    {
        return _handler.ToStringAndClear();
    }

    public void AppendLiteral(string value)
    {
        _handler.AppendLiteral(value);
    }

    public void AppendFormatted(object? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        _handler.AppendFormatted(value, format);
    }

    public void AppendFormatted<T>(T value, int alignment)
    {
        _handler.AppendFormatted(value, alignment);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }
}

[InterpolatedStringHandler]
public ref struct ErrorInterpolatedStringHandler
{
    private LogInterpolatedStringHandler _handler;
    public readonly bool IsEnabled;
    
    public ErrorInterpolatedStringHandler(int literalLength, int formattedCount, Logger logger,out bool isEnabled,
        [CallerFilePath] string filePath = "", [CallerMemberName] string method = "", [CallerLineNumber] int line = 0)
    {
        if (!logger.LogMgr.CheckLevel(LogLevel.Error))
        {
            _handler = default;
            isEnabled = false;
            IsEnabled = isEnabled;
            return;
        }

        isEnabled = true;
        IsEnabled = isEnabled;
        _handler = new LogInterpolatedStringHandler(literalLength, formattedCount, "[E]", logger.Tag, filePath, method, line);
    }
    
    public string ToStringAndClear()
    {
        return _handler.ToStringAndClear();
    }

    public void AppendLiteral(string value)
    {
        _handler.AppendLiteral(value);
    }

    public void AppendFormatted(object? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(string? value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        _handler.AppendFormatted(value);
    }

    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        _handler.AppendFormatted(value, alignment, format);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        _handler.AppendFormatted(value, format);
    }

    public void AppendFormatted<T>(T value, int alignment)
    {
        _handler.AppendFormatted(value, alignment);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }
}

internal ref struct LogInterpolatedStringHandler
{
    private const int MaxLength = 0x3FFFFFDF;
    private const int GuessedLengthPerHole = 20;
    private const int DateTimeLength = 30;
    private const int LevelLength = 5;
    private const int ThreadLength = 5;
    private const int MinimumArrayPoolLength = 256;
    private char[]? _arrayToReturnToPool;
    private Span<char> _chars;
    private int _pos;
    
    public LogInterpolatedStringHandler(int literalLength, int formattedCount, string level, string tag, 
        string filePath, string method, int line)
    {
        var fileNameSpan = Path.GetFileName(filePath.AsSpan());

        literalLength = literalLength + DateTimeLength + LevelLength + ThreadLength;
        literalLength += tag.Length + 2;
        literalLength += fileNameSpan.Length + method.Length + 8;

        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;

        // 内置的日志格式，如果有定制需求直接改这里即可
        // [23-11-02 11:04:34.070][D][30][MyClass.cs@38:Start][Tag]
        // AppendFormatted(DateTimeOffset.Now, "[yy-MM-dd HH:mm:ss.fff zzz]");
        AppendFormatted(DateTimeOffset.Now, "[yy-MM-dd HH:mm:ss.fff]");
        AppendLiteral(level);

        AppendLiteral("[");
        AppendFormatted(Environment.CurrentManagedThreadId);
        AppendLiteral("]");
        
        AppendLiteral("[");
        AppendFormatted(fileNameSpan);
        AppendLiteral("@");
        AppendFormatted(line);
        AppendLiteral(":");
        AppendFormatted(method);
        AppendLiteral("]");
        
        AppendLiteral("[");
        AppendFormatted(tag);
        AppendLiteral("]");
    }

    /// <summary>Derives a default length with which to seed the handler.</summary>
    /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // becomes a constant when inputs are constant
    private static int GetDefaultLength(int literalLength, int formattedCount) =>
        Math.Max(MinimumArrayPoolLength, literalLength + (formattedCount * GuessedLengthPerHole));

    /// <summary>Gets the built <see cref="string"/> and clears the handler.</summary>
    /// <returns>The built string.</returns>
    /// <remarks>
    /// This releases any resources used by the handler. The method should be invoked only
    /// once and as the last thing performed on the handler. Subsequent use is erroneous, ill-defined,
    /// and may destabilize the process, as may using any other copies of the handler after ToStringAndClear
    /// is called on any one of them.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToStringAndClear()
    {
        string result = new string(_chars[.._pos]);
        Clear();
        return result;
    }

    /// <summary>Clears the handler, returning any rented array to the pool.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // used only on a few hot paths
    private void Clear()
    {
        char[]? toReturn = _arrayToReturnToPool;
        this = default; // defensive clear
        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
    
    /// <summary>Writes the specified string to the handler.</summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLiteral(string value)
    {
        // AppendLiteral is expected to always be called by compiler-generated code with a literal string.
        // By inlining it, the method body is exposed to the constant length of that literal, allowing the JIT to
        // prune away the irrelevant cases.  This effectively enables multiple implementations of AppendLiteral,
        // special-cased on and optimized for the literal's length.  We special-case lengths 1 and 2 because
        // they're very common, e.g.
        //     1: ' ', '.', '-', '\t', etc.
        //     2: ", ", "0x", "=>", ": ", etc.
        // but we refrain from adding more because, in the rare case where AppendLiteral is called with a non-literal,
        // there is a lot of code here to be inlined.

        // TODO: https://github.com/dotnet/runtime/issues/41692#issuecomment-685192193
        // What we really want here is to be able to add a bunch of additional special-cases based on length,
        // e.g. a switch with a case for each length <= 8, not mark the method as AggressiveInlining, and have
        // it inlined when provided with a string literal such that all the other cases evaporate but not inlined
        // if called directly with something that doesn't enable pruning.  Even better, if "literal".TryCopyTo
        // could be unrolled based on the literal, ala https://github.com/dotnet/runtime/pull/46392, we might
        // be able to remove all special-casing here.

        if (value.Length == 1)
        {
            Span<char> chars = _chars;
            int pos = _pos;
            if ((uint) pos < (uint) chars.Length)
            {
                chars[pos] = value[0];
                _pos = pos + 1;
            }
            else
            {
                GrowThenCopyString(value);
            }

            return;
        }

        AppendStringDirect(value);
    }

    /// <summary>Writes the specified string to the handler.</summary>
    /// <param name="value">The string to write.</param>
    private void AppendStringDirect(string value)
    {
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopyString(value);
        }
    }

    #region AppendFormatted

    // Design note:
    // The compiler requires a AppendFormatted overload for anything that might be within an interpolation expression;
    // if it can't find an appropriate overload, for handlers in general it'll simply fail to compile.
    // (For target-typing to string where it uses LogInterpolatedStringHandler implicitly, it'll instead fall back to
    // its other mechanisms, e.g. using string.Format.  This fallback has the benefit that if we miss a case,
    // interpolated strings will still work, but it has the downside that a developer generally won't know
    // if the fallback is happening and they're paying more.)
    //
    // At a minimum, then, we would need an overload that accepts:
    //     (object value, int alignment = 0, string? format = null)
    // Such an overload would provide the same expressiveness as string.Format.  However, this has several
    // shortcomings:
    // - Every value type in an interpolation expression would be boxed.
    // - ReadOnlySpan<char> could not be used in interpolation expressions.
    // - Every AppendFormatted call would have three arguments at the call site, bloating the IL further.
    // - Every invocation would be more expensive, due to lack of specialization, every call needing to account
    //   for alignment and format, etc.
    //
    // To address that, we could just have overloads for T and ReadOnlySpan<char>:
    //     (T)
    //     (T, int alignment)
    //     (T, string? format)
    //     (T, int alignment, string? format)
    //     (ReadOnlySpan<char>)
    //     (ReadOnlySpan<char>, int alignment)
    //     (ReadOnlySpan<char>, string? format)
    //     (ReadOnlySpan<char>, int alignment, string? format)
    // but this also has shortcomings:
    // - Some expressions that would have worked with an object overload will now force a fallback to string.Format
    //   (or fail to compile if the handler is used in places where the fallback isn't provided), because the compiler
    //   can't always target type to T, e.g. `b switch { true => 1, false => null }` where `b` is a bool can successfully
    //   be passed as an argument of type `object` but not of type `T`.
    // - Reference types get no benefit from going through the generic code paths, and actually incur some overheads
    //   from doing so.
    // - Nullable value types also pay a heavy price, in particular around interface checks that would generally evaporate
    //   at compile time for value types but don't (currently) if the Nullable<T> goes through the same code paths
    //   (see https://github.com/dotnet/runtime/issues/50915).
    //
    // We could try to take a more elaborate approach for LogInterpolatedStringHandler, since it is the most common handler
    // and we want to minimize overheads both at runtime and in IL size, e.g. have a complete set of overloads for each of:
    //     (T, ...) where T : struct
    //     (T?, ...) where T : struct
    //     (object, ...)
    //     (ReadOnlySpan<char>, ...)
    //     (string, ...)
    // but this also has shortcomings, most importantly:
    // - If you have an unconstrained T that happens to be a value type, it'll now end up getting boxed to use the object overload.
    //   This also necessitates the T? overload, since nullable value types don't meet a T : struct constraint, so without those
    //   they'd all map to the object overloads as well.
    // - Any reference type with an implicit cast to ROS<char> will fail to compile due to ambiguities between the overloads. string
    //   is one such type, hence needing dedicated overloads for it that can be bound to more tightly.
    //
    // A middle ground we've settled on, which is likely to be the right approach for most other handlers as well, would be the set:
    //     (T, ...) with no constraint
    //     (ReadOnlySpan<char>) and (ReadOnlySpan<char>, int)
    //     (object, int alignment = 0, string? format = null)
    //     (string) and (string, int)
    // This would address most of the concerns, at the expense of:
    // - Most reference types going through the generic code paths and so being a bit more expensive.
    // - Nullable types being more expensive until https://github.com/dotnet/runtime/issues/50915 is addressed.
    //   We could choose to add a T? where T : struct set of overloads if necessary.
    // Strings don't require their own overloads here, but as they're expected to be very common and as we can
    // optimize them in several ways (can copy the contents directly, don't need to do any interface checks, don't
    // need to pay the shared generic overheads, etc.) we can add overloads specifically to optimize for them.
    //
    // Hole values are formatted according to the following policy:
    // 1. If an IFormatProvider was supplied and it provides an ICustomFormatter, use ICustomFormatter.Format (even if the value is null).
    // 2. If the type implements ISpanFormattable, use ISpanFormattable.TryFormat.
    // 3. If the type implements IFormattable, use IFormattable.ToString.
    // 4. Otherwise, use object.ToString.
    // This matches the behavior of string.Format, StringBuilder.AppendFormat, etc.  The only overloads for which this doesn't
    // apply is ReadOnlySpan<char>, which isn't supported by either string.Format nor StringBuilder.AppendFormat, but more
    // importantly which can't be boxed to be passed to ICustomFormatter.Format.

    #region AppendFormatted T

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value)
    {
        // Check first for IFormattable, even though we'll prefer to use ISpanFormattable, as the latter
        // requires the former.  For value types, it won't matter as the type checks devolve into
        // JIT-time constants.  For reference types, they're more likely to implement IFormattable
        // than they are to implement ISpanFormattable: if they don't implement either, we save an
        // interface check over first checking for ISpanFormattable and then for IFormattable, and
        // if it only implements IFormattable, we come out even: only if it implements both do we
        // end up paying for an extra interface check.
        string? s;
        if (value is IFormattable)
        {
            // If the value can format itself directly into our buffer, do so.
            if (value is ISpanFormattable)
            {
                int charsWritten;
                while (!((ISpanFormattable) value).TryFormat(_chars.Slice(_pos), out charsWritten, default, null)) // constrained call avoiding boxing for value types
                {
                    Grow();
                }

                _pos += charsWritten;
                return;
            }

            s = ((IFormattable) value).ToString(format: null, null); // constrained call avoiding boxing for value types
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendStringDirect(s);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value, string? format)
    {
        // Check first for IFormattable, even though we'll prefer to use ISpanFormattable, as the latter
        // requires the former.  For value types, it won't matter as the type checks devolve into
        // JIT-time constants.  For reference types, they're more likely to implement IFormattable
        // than they are to implement ISpanFormattable: if they don't implement either, we save an
        // interface check over first checking for ISpanFormattable and then for IFormattable, and
        // if it only implements IFormattable, we come out even: only if it implements both do we
        // end up paying for an extra interface check.
        string? s;
        if (value is IFormattable)
        {
            // If the value can format itself directly into our buffer, do so.
            if (value is ISpanFormattable)
            {
                int charsWritten;
                while (!((ISpanFormattable) value).TryFormat(_chars.Slice(_pos), out charsWritten, format,
                           null)) // constrained call avoiding boxing for value types
                {
                    Grow();
                }

                _pos += charsWritten;
                return;
            }

            s = ((IFormattable) value).ToString(format, null); // constrained call avoiding boxing for value types
        }
        else
        {
            s = value?.ToString();
        }

        if (s is not null)
        {
            AppendStringDirect(s);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value, int alignment)
    {
        int startingPos = _pos;
        AppendFormatted(value);
        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format string.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted<T>(T value, int alignment, string? format)
    {
        int startingPos = _pos;
        AppendFormatted(value, format);
        if (alignment != 0)
        {
            AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
        }
    }

    #endregion

    #region AppendFormatted ReadOnlySpan<char>

    /// <summary>Writes the specified character span to the handler.</summary>
    /// <param name="value">The span to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(ReadOnlySpan<char> value)
    {
        // Fast path for when the value fits in the current buffer
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopySpan(value);
        }
    }

    /// <summary>Writes the specified string of chars to the handler.</summary>
    /// <param name="value">The span to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
    {
        bool leftAlign = false;
        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingRequired = alignment - value.Length;
        if (paddingRequired <= 0)
        {
            // The value is as large or larger than the required amount of padding,
            // so just write the value.
            AppendFormatted(value);
            return;
        }

        // Write the value along with the appropriate padding.
        EnsureCapacityForAdditionalChars(value.Length + paddingRequired);
        if (leftAlign)
        {
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;
        }
        else
        {
            _chars.Slice(_pos, paddingRequired).Fill(' ');
            _pos += paddingRequired;
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }
    }

    #endregion

    #region AppendFormatted string

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendFormatted(string? value)
    {
        // Fast-path for no custom formatter and a non-null string that fits in the current destination buffer.
        if (value is not null &&
            value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            AppendFormattedSlow(value);
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <remarks>
    /// Slow path to handle a custom formatter, potentially null value,
    /// or a string that doesn't fit in the current buffer.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendFormattedSlow(string? value)
    {
        if (value is not null)
        {
            EnsureCapacityForAdditionalChars(value.Length);
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }
    }

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(string? value, int alignment = 0, string? format = null) =>
        // Format is meaningless for strings and doesn't make sense for someone to specify.  We have the overload
        // simply to disambiguate between ROS<char> and object, just in case someone does specify a format, as
        // string is implicitly convertible to both. Just delegate to the T-based implementation.
        AppendFormatted<string?>(value, alignment, format);

    #endregion

    #region AppendFormatted object

    /// <summary>Writes the specified value to the handler.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="alignment">Minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    /// <param name="format">The format string.</param>
    public void AppendFormatted(object? value, int alignment = 0, string? format = null) =>
        // This overload is expected to be used rarely, only if either a) something strongly typed as object is
        // formatted with both an alignment and a format, or b) the compiler is unable to target type to T. It
        // exists purely to help make cases from (b) compile. Just delegate to the T-based implementation.
        AppendFormatted<object?>(value, alignment, format);

    #endregion

    #endregion

    /// <summary>Handles adding any padding required for aligning a formatted value in an interpolation expression.</summary>
    /// <param name="startingPos">The position at which the written value started.</param>
    /// <param name="alignment">Non-zero minimum number of characters that should be written for this value.  If the value is negative, it indicates left-aligned and the required minimum is the absolute value.</param>
    private void AppendOrInsertAlignmentIfNeeded(int startingPos, int alignment)
    {
        Debug.Assert(startingPos >= 0 && startingPos <= _pos);
        Debug.Assert(alignment != 0);

        int charsWritten = _pos - startingPos;

        bool leftAlign = false;
        if (alignment < 0)
        {
            leftAlign = true;
            alignment = -alignment;
        }

        int paddingNeeded = alignment - charsWritten;
        if (paddingNeeded > 0)
        {
            EnsureCapacityForAdditionalChars(paddingNeeded);

            if (leftAlign)
            {
                _chars.Slice(_pos, paddingNeeded).Fill(' ');
            }
            else
            {
                _chars.Slice(startingPos, charsWritten).CopyTo(_chars.Slice(startingPos + paddingNeeded));
                _chars.Slice(startingPos, paddingNeeded).Fill(' ');
            }

            _pos += paddingNeeded;
        }
    }

    /// <summary>Ensures <see cref="_chars"/> has the capacity to store <paramref name="additionalChars"/> beyond <see cref="_pos"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacityForAdditionalChars(int additionalChars)
    {
        if (_chars.Length - _pos < additionalChars)
        {
            Grow(additionalChars);
        }
    }

    /// <summary>Fallback for fast path in <see cref="AppendStringDirect"/> when there's not enough space in the destination.</summary>
    /// <param name="value">The string to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopyString(string value)
    {
        Grow(value.Length);
        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>Fallback for <see cref="AppendFormatted(ReadOnlySpan{char})"/> for when not enough space exists in the current buffer.</summary>
    /// <param name="value">The span to write.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopySpan(ReadOnlySpan<char> value)
    {
        Grow(value.Length);
        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    /// <summary>Grows <see cref="_chars"/> to have the capacity to store at least <paramref name="additionalChars"/> beyond <see cref="_pos"/>.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void Grow(int additionalChars)
    {
        // This method is called when the remaining space (_chars.Length - _pos) is
        // insufficient to store a specific number of additional characters.  Thus, we
        // need to grow to at least that new total. GrowCore will handle growing by more
        // than that if possible.
        Debug.Assert(additionalChars > _chars.Length - _pos);
        GrowCore((uint) _pos + (uint) additionalChars);
    }

    /// <summary>Grows the size of <see cref="_chars"/>.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void Grow()
    {
        // This method is called when the remaining space in _chars isn't sufficient to continue
        // the operation.  Thus, we need at least one character beyond _chars.Length.  GrowCore
        // will handle growing by more than that if possible.
        GrowCore((uint) _chars.Length + 1);
    }

    /// <summary>Grow the size of <see cref="_chars"/> to at least the specified <paramref name="requiredMinCapacity"/>.</summary>
    [MethodImpl(MethodImplOptions
        .AggressiveInlining)] // but reuse this grow logic directly in both of the above grow routines
    private void GrowCore(uint requiredMinCapacity)
    {
        // We want the max of how much space we actually required and doubling our capacity (without going beyond the max allowed length). We
        // also want to avoid asking for small arrays, to reduce the number of times we need to grow, and since we're working with unsigned
        // ints that could technically overflow if someone tried to, for example, append a huge string to a huge string, we also clamp to int.MaxValue.
        // Even if the array creation fails in such a case, we may later fail in ToStringAndClear.

        uint newCapacity = Math.Max(requiredMinCapacity, Math.Min((uint) _chars.Length * 2, MaxLength));
        int arraySize = (int) Math.Clamp(newCapacity, MinimumArrayPoolLength, int.MaxValue);

        char[] newArray = ArrayPool<char>.Shared.Rent(arraySize);
        _chars.Slice(0, _pos).CopyTo(newArray);

        char[]? toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = newArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}