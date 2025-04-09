// Written by Colin on 2025-02-20

using Microsoft.Extensions.Configuration.Json;
using NetEscapades.Configuration.Yaml;
using Winton.Extensions.Configuration.Consul.Parsers;

namespace CoRuntime.Common;

/// <summary>
/// Yaml配置解析器，用于支持Consul的Yaml配置
/// </summary>
public class YamlConfigurationParser : IConfigurationParser
{
    public IDictionary<string, string> Parse(Stream stream)
    {
        return YamlParser.Parse(stream);
    }
    
    private sealed class YamlParser : YamlConfigurationProvider
    {
        public YamlParser(YamlConfigurationSource source) : base(source)
        {
        }

        internal static IDictionary<string, string> Parse(Stream stream)
        {
            var parser = new YamlParser(new YamlConfigurationSource());
            parser.Load(stream);
            return parser.Data!;
        }
    }
}