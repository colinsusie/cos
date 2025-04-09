using CoLib.Container;
using CoRuntime.Services;

namespace TimeServer.TimeService;

public class TimerServicePlugin : IServicePlugin
{
    public Service CreateService(ServiceContext context)
    {
        if (context.ServiceName == nameof(TimeService))
            return new TimeService(context);
        throw new NotSupportedException($"Cannot create a service named {context.ServiceName}");
    }
}