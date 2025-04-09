// Written by Colin on 2024-10-11

using System.Runtime.CompilerServices;
using System.Text;

namespace CoLib.Extensions;

public enum NumBaseType
{
    Base2,
    Base10,
    Base16,
}

public static class SpanExt
{
    public static string GetPrintableHex(this ReadOnlySpan<byte> span, int colNum = 8, bool upperCase = true, 
        NumBaseType baseType = NumBaseType.Base16)
    {
        var strHandler = new DefaultInterpolatedStringHandler(span.Length * 3, 0);
        var idx = 0;

        var format = baseType switch
        {
            NumBaseType.Base2 => "b8",
            NumBaseType.Base10 => "000",
            _ => upperCase ? "X" : "x",
        };

        foreach (var b in span)
        {
            idx++;
            strHandler.AppendFormatted(b, format);

            if (idx % colNum == 0)
            {
                strHandler.AppendFormatted('\n');
                idx = 0;
            }
            else
            {
                strHandler.AppendFormatted(' ');
            }
        }

        return strHandler.ToString();
    }
}