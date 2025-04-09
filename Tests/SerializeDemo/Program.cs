// See https://aka.ms/new-console-template for more information
// 序列化的Demo

using System.Buffers;
using Google.Protobuf;
using SerializeDemo;

Console.WriteLine("Hello, World!");

// 下面例子加载PB文件，并生成描述信息
// using var stream = File.OpenRead(@"f:\cos\Tests\SerializeDemo\ProtoDesc\desc.pb");
// FileDescriptorSet descriptorSet = FileDescriptorSet.Parser.ParseFrom(stream);
// var byteStrings = descriptorSet.File.Select(f => f.ToByteString()).ToList();
// var descriptors = FileDescriptor.BuildFromByteStrings(byteStrings);
// foreach (var desc in descriptors)
// {
//     Console.WriteLine($"{desc.MessageTypes.Count}");
// }

var buffer = new ArrayBufferWriter<byte>();
var pb = new PbPlayer();
pb.WriteTo(buffer);
pb.MergeFrom(buffer.WrittenSpan);