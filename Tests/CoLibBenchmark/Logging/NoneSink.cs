// Written by Colin on 2023-11-03

using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace CoLibBenchmark.Logging;

class NoneSink : ILogEventSink
{
    readonly ITextFormatter _formatter;

    const int DefaultWriteBufferCapacity = 256;

    public NoneSink(ITextFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Emit(LogEvent logEvent)
    {
        var buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
        _formatter.Format(logEvent, buffer);
        var str = buffer.ToString();
        Output(str);
    }

    public string Output(string str)
    {
        return str;
    }
}