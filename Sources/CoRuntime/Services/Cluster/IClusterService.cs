// Written by Colin on 2025-02-04

using CoRuntime.Rpc;

namespace CoRuntime.Services.Cluster;

/// <summary>
/// Consul服务接口
/// </summary>
public interface IClusterService: IRpcService
{
    /// 更新服务
    void RegisterService(string serviceName, short serviceId);
}