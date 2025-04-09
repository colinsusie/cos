using CoRuntime.Services;

namespace ClientApp.ClientService;


public class ClientServicePlugin: IServicePlugin
{
    public Service CreateService(ServiceContext context)
    {
        if (context.ServiceName == nameof(ClientService))
            return new ClientService(context);
        throw new NotSupportedException($"Cannot create a service named {context.ServiceName}");
    }
}