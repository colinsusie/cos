// Written by Colin on 2024-10-16

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CoLib.Container;

namespace CoLib.Serialize;

public static class CoPackConverter
{
    public static string ToJson(ReadOnlySpan<byte> buffer, bool indented = true)
    {
        var reader = new CoPackReader(buffer);
        using var jsonBufferWriter = new PooledByteBufferWriter();
        using var jsonWriter = new Utf8JsonWriter(jsonBufferWriter, new JsonWriterOptions
        {
            Indented = indented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        DoToJson(ref reader, jsonWriter, 0);
        jsonWriter.Flush();
        return Encoding.UTF8.GetString(jsonBufferWriter.WrittenSpan);
    }
    
    private static void DoToJson(ref CoPackReader reader, Utf8JsonWriter jsonWriter, int depth)
    {
        if (reader.Remaining == 0)
            return;
        
        if (depth >= 255)
        {
            CoPackException.ThrowReachedDepthLimit();
        }

        var (type, cookie) = reader.PeekHeader();
        switch (type)
        {
            case CoPackType.NullBool:
            {
                reader.Skip(1);
                switch (cookie)
                {
                    case (int)CookieNullBoolType.Null:
                        jsonWriter.WriteNullValue();
                        break;
                    case (int)CookieNullBoolType.False:
                        jsonWriter.WriteBooleanValue(false);
                        break;
                    case (int)CookieNullBoolType.True:
                        jsonWriter.WriteBooleanValue(true);
                        break;
                    default:
                        CoPackException.ThrowReadUnexpectedBool(cookie);
                        break;
                }
                break;
            }
            case CoPackType.Int:
            {
                switch (cookie)
                {
                    case <= CoPackCode.CookieIntMaxValue:
                        reader.Skip(1);
                        jsonWriter.WriteNumberValue(cookie);
                        break;
                    case (int)CookieIntType.Int8:
                        jsonWriter.WriteNumberValue(reader.ReadInt8());
                        break;
                    case (int)CookieIntType.UInt8:
                        jsonWriter.WriteNumberValue(reader.ReadUInt8());
                        break;
                    case (int)CookieIntType.Int16:
                        jsonWriter.WriteNumberValue(reader.ReadInt16());
                        break;
                    case (int)CookieIntType.UInt16:
                        jsonWriter.WriteNumberValue(reader.ReadUInt16());
                        break;
                    case (int)CookieIntType.Int32:
                        jsonWriter.WriteNumberValue(reader.ReadInt32());
                        break;
                    case (int)CookieIntType.UInt32:
                        jsonWriter.WriteNumberValue(reader.ReadUInt32());
                        break;
                    case (int)CookieIntType.Int64:
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteString("type", "int64");
                        jsonWriter.WriteString("value", reader.ReadInt64().ToString());
                        jsonWriter.WriteEndObject();
                        break;
                    case (int)CookieIntType.UInt64:
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteString("type", "uint64");
                        jsonWriter.WriteString("value", reader.ReadUInt64().ToString());
                        jsonWriter.WriteEndObject();
                        break;
                    default:
                        CoPackException.ThrowReadUnexpectedInt(cookie);
                        break;
                }
                break;
            }
            case CoPackType.Float:
            {
                switch (cookie)
                {
                    case 0:
                        reader.Skip(1);
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteString("type", "float");
                        jsonWriter.WriteNumber("value", 0.0f);
                        jsonWriter.WriteEndObject();
                        break;
                    case (int)CookieFloatType.Float:
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteString("type", "float");
                        jsonWriter.WriteNumber("value", reader.ReadFloat());
                        jsonWriter.WriteEndObject();
                        break;
                    case (int)CookieFloatType.Double:
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteString("type", "double");
                        jsonWriter.WriteNumber("value", reader.ReadDouble());
                        jsonWriter.WriteEndObject();
                        break;
                    default:
                        CoPackException.ThrowReadUnexpectedFloat(cookie);
                        break;
                }
                break;
            }
            case CoPackType.List:
            {
                var len = reader.ReadListHeader();
                jsonWriter.WriteStartArray();
                for (var i = 0; i < len; i++)
                {
                    DoToJson(ref reader, jsonWriter, depth+1);
                }
                jsonWriter.WriteEndArray();
                break;
            }
            case CoPackType.Map:
            {
                if (cookie == 0)
                {
                    reader.ReadObjectHeader();
                    jsonWriter.WriteStartObject();
                    jsonWriter.WriteString("type", "object");
                    jsonWriter.WritePropertyName("values");
                    jsonWriter.WriteStartArray();

                    while (!reader.TryReadNull())
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName("tag");
                        DoToJson(ref reader, jsonWriter, depth+1);
                        jsonWriter.WritePropertyName("value");
                        DoToJson(ref reader, jsonWriter, depth+1);
                        jsonWriter.WriteEndObject();
                    }
                 
                    jsonWriter.WriteEndArray();   
                    jsonWriter.WriteEndObject();
                }
                else
                {
                    var len = reader.ReadMapHeader();
                    jsonWriter.WriteStartObject();
                    jsonWriter.WriteString("type", "map");
                    jsonWriter.WritePropertyName("values");
                    jsonWriter.WriteStartArray();
                    
                    for (var i = 0; i < len; ++i)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName("key");
                        DoToJson(ref reader, jsonWriter, depth+1);
                        jsonWriter.WritePropertyName("value");
                        DoToJson(ref reader, jsonWriter, depth+1);
                        jsonWriter.WriteEndObject();
                    }
                
                    jsonWriter.WriteEndArray();   
                    jsonWriter.WriteEndObject();
                }
                break;
            }
            case CoPackType.Union:
            {
                var tag = reader.ReadUnionHeader();
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("type", "union");
                jsonWriter.WriteNumber("tag", tag);
                jsonWriter.WritePropertyName("value");
                DoToJson(ref reader, jsonWriter, depth+1);
                jsonWriter.WriteEndObject();
                break;
            }
            case CoPackType.Bytes:
            {
                var bytes = reader.ReadBytes()!;
                var base64 = Convert.ToBase64String(bytes);
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("type", "bytes");
                jsonWriter.WriteString("value", base64);
                jsonWriter.WriteEndObject();
                break;
            }
            case CoPackType.String:
            {
                jsonWriter.WriteStringValue(reader.ReadString()!);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}