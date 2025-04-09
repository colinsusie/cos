// Written by Colin on 2023-11-03

using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;

namespace CoLibBenchmark.Logging;

public static class NoneLoggerConfigurationExtensions
{
    private const string DefaultConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration None(
        this LoggerSinkConfiguration sinkConfiguration,
        ITextFormatter formatter)
    {
        if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
        return sinkConfiguration.Sink(new NoneSink(formatter));
    }
}