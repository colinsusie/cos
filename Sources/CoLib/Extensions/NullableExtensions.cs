// colin 2023-05-14

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CoLib.Common;

namespace CoLib.Extensions;

/// <summary>
/// 用于保证值不为空
/// </summary>
public static class NullableExtensions
{
    /// <summary>
    /// 传入值必须为非null，否则抛出异常
    /// </summary>
    /// <param name="value">传入值</param>
    /// <param name="parameter">参数名，自动生成</param>
    /// <returns>非null值</returns>
    /// <exception cref="ArgumentNullException">参数null异常</exception>
    public static T NotNull<T>([NotNull] this T? value,
        [CallerArgumentExpression("value")] string? parameter = null) where T : class
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNullException(parameter);
        return value;
    }

    /// <summary>
    /// 传入值必须为非null，否则抛出异常
    /// </summary>
    /// <param name="value">传入值</param>
    /// <param name="parameter">参数名，自动生成</param>
    /// <returns>非null值</returns>
    /// <exception cref="ArgumentNullException">参数null异常</exception>
    public static T NotNull<T>([NotNull] this T? value,
        [CallerArgumentExpression("value")] string? parameter = null) where T : struct
    {
        if (value is null)
            ThrowHelper.ThrowArgumentNullException(parameter);
        return value.Value;
    }
}