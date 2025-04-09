using CoLib.Container;
using CoRuntime.Services;

namespace CacheService;

public class CacheServicePlugin: IServicePlugin
{
    public Service CreateService(ServiceContext context)
    {
        if (context.ServiceName == nameof(CacheService))
            return new CacheService(context);
        throw new NotSupportedException($"Cannot create a service named {context.ServiceName}");
    }
}