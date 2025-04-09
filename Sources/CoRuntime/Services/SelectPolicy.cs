// Written by Colin on 2025-02-11

using CoLib.Serialize;

namespace CoRuntime.Services;

/// <summary>
/// 服务选择策略
/// </summary>
public enum ServiceSelectPolicy
{
    First,                  // 第一个
    Random,                 // 随机
}