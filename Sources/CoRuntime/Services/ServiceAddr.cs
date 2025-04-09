// Written by Colin on 2025-02-06

using System.Text.Json.Serialization;
using CoLib.Serialize;

namespace CoRuntime.Services;

/// <summary>
/// 服务地址
/// </summary>
[CoPackable]
public partial record struct ServiceAddr
{
    [Tag(1)] public string NodeName { get; set; }
    [Tag(2)] public short ServiceId { get; set; }
}