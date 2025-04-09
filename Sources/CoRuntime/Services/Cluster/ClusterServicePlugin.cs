// Written by Colin on 2025-02-04

using CoLib.Container;

namespace CoRuntime.Services.Cluster;

public class ClusterServicePlugin: IServicePlugin
{
    public Service CreateService(ServiceContext context)
    {
        return new ClusterService(context);
    }
}