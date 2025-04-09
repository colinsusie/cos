// Written by Colin on 2023-11-05

using System.Diagnostics;

namespace CoLib.Extensions;

/// <summary>
/// 异常扩展
/// </summary>
public static class ExceptionExt
{
    /// <summary>
    /// 返回异常的全堆栈
    /// </summary>
    /// <param name="ex"></param>
    public static string GetFullStacktrace(this Exception ex)
    {
        var exStr = ex.ToString();
        var stack = new StackTrace(ex.StackTrace != null ? 2 : 1, true);
        return exStr + "\n" + stack;
    }
}