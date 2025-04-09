// Written by Colin on 2025-02-20

namespace CoRuntime.Services.Uid;

public class UidServicePlugin: IServicePlugin
{
    public Service CreateService(ServiceContext context)
    {
        return new UidService(context);
    }
}