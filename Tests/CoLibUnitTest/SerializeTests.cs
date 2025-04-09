// Written by Colin on 2024-10-8

using System.Buffers;
using System.Numerics;
using CoLib.Container;
using CoLib.Serialize;
using Xunit.Abstractions;

namespace CoLibUnitTest;

public class SerializeTests
{
    private readonly ITestOutputHelper _output;

    public SerializeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(DisplayName = "读写Null值")]
    public void WriteReadNull()
    {
        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        writer.WriteNull();

        Assert.True(buffer.WrittenSpan.Length == 1);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b0000_0000}));

        var reader = new CoPackReader(buffer.WrittenSpan);
        Assert.True(reader.TryReadNull());
        Assert.True(reader.Consumed == 1);
        
        buffer.Dispose();
    }

    [Fact(DisplayName = "读写Bool值")]
    public void WriteReadBool()
    {
        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        writer.WriteBool(false);

        Assert.True(buffer.WrittenSpan.Length == 1);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b0000_0001}));

        writer.WriteBool(true);
        Assert.True(buffer.WrittenSpan.Length == 2);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b0000_0001, 0b0000_0010}));

        var reader = new CoPackReader(buffer.WrittenSpan);
        var value = reader.ReadBool();
        Assert.True(value == false);
        Assert.True(reader.Consumed == 1);
        value = reader.ReadBool();
        Assert.True(value == true);
        Assert.True(reader.Consumed == 2);
        
        buffer.Dispose();
    }

    [Fact(DisplayName = "读写可变的sbyte值")]
    public void WriteReadVarSByte()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((sbyte) 0);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00000}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt8();
            Assert.True(value == 0);
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((sbyte) 7);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((sbyte) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((sbyte) -1);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10000, 0b1111_1111}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt8();
            Assert.True(value == -1);
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的byte值")]
    public void WriteReadVarByte()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((byte) 0);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00000}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((byte) 7);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((byte) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((byte) 127);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 127}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt32();
            Assert.True(value == 127);
            
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的short值")]
    public void WriteReadVarShort()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((short) 0);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00000}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((short) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((short) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((short) 0x0F01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10010, 0x01, 0x0F}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((short) -2);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10010, 0xFE, 0xFF}));
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的ushort值")]
    public void WriteReadVarUShort()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ushort) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ushort) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ushort) 0xFF01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10011, 0x01, 0xFF}));
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的int值")]
    public void WriteReadVarInt()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((int) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((int) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((int) 0x0F01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10011, 0x01, 0x0F}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadUInt16();
            Assert.True(value == 0x0F01);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((int) 0xFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10100, 0x01, 0x0F, 0xFF, 0x00}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt32();
            Assert.True(value == 0xFF0F01);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((int) -2);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10100, 0xFE, 0xFF, 0xFF, 0xFF}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt32();
            Assert.True(value == -2);
            
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的uint值")]
    public void WriteReadVarUInt()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((uint) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((uint) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((uint) 0x0F01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10011, 0x01, 0x0F}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((uint) 0xFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10101, 0x01, 0x0F, 0xFF, 0x00}));
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的long值")]
    public void WriteReadVarLong()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) 0x0F01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10011, 0x01, 0x0F}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) 0xFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10101, 0x01, 0x0F, 0xFF, 0x00}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) 0x7FFFFF01FFFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 9);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
                {0b001_10110, 0x01, 0x0F, 0xFF, 0xFF, 0x01, 0xFF, 0xFF, 0x7F}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadInt64();
            Assert.True(value == 0x7FFFFF01FFFF0F01);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((long) -2);
            Assert.True(buffer.WrittenSpan.Length == 9);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
                {0b001_10110, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}));
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写可变的ulong值")]
    public void WriteReadVarULong()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ulong) 15);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_01111}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ulong) 160);
            Assert.True(buffer.WrittenSpan.Length == 2);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10001, 160}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ulong) 0x0F01);
            Assert.True(buffer.WrittenSpan.Length == 3);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10011, 0x01, 0x0F}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ulong) 0xFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_10101, 0x01, 0x0F, 0xFF, 0x00}));
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteVarInt((ulong) 0x7FFFFF01FFFF0F01);
            Assert.True(buffer.WrittenSpan.Length == 9);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
                {0b001_10111, 0x01, 0x0F, 0xFF, 0xFF, 0x01, 0xFF, 0xFF, 0x7F}));
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写float")]
    public void WriteReadFloat()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteFloat((float) 0);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b010_00000}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadFloat();
            Assert.True(value == 0);
            Assert.True(reader.Consumed == 1);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteFloat((float) 0.1234);
            Assert.True(buffer.WrittenSpan.Length == 5);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b010_00001, 0x24, 0xb9, 0xfc, 0x3d}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadFloat();
            Assert.True(Math.Abs(value - 0.1234) < 0.00001);
            Assert.True(reader.Consumed == 5);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteFloat((double) 0);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b010_00000}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadDouble();
            Assert.True(value == 0);
            Assert.True(reader.Consumed == 1);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteFloat((double) 0.525);
            Assert.True(buffer.WrittenSpan.Length == 9);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
                {0b010_00010, 0xcd, 0xcc, 0xcc, 0xcc, 0xcc, 0xcc, 0xe0, 0x3f}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadDouble();
            Assert.True(Math.Abs(value - 0.525) < 0.00000001);
            Assert.True(reader.Consumed == 9);
            
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写string")]
    public void WriteReadString()
    {
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteString("abc");
            Assert.True(buffer.WrittenSpan.Length == 4);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b111_00011, 97, 98, 99}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadString();
            Assert.True(value == "abc");
            Assert.True(reader.Consumed == 4);
            
            buffer.Dispose();
        }

        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            writer.WriteString("你好呀，hello");
            Assert.True(buffer.WrittenSpan.Length == 19);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
            {
                0b111_10001, 17,
                228, 189, 160, 229, 165, 189, 229, 145, 128, 239, 188, 140, 104, 101, 108, 108, 111
            }));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var value = reader.ReadString();
            Assert.True(value == "你好呀，hello");
            Assert.True(reader.Consumed == 19);
            
            buffer.Dispose();
        }
    }

    [Fact(DisplayName = "读写Enum")]
    public void WriteReadEnum()
    {
        CoPackFormatterProvider.Register(new EnumFormatter<Color>());
        {
            {
                var buffer = new PooledByteBufferWriter();
                var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

                Color color = Color.Blue;
                writer.WriteValue(color, PackFlags.All);

                Assert.True(buffer.WrittenSpan.Length == 1);
                Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b001_00001}));

                var reader = new CoPackReader(buffer.WrittenSpan);
                var value = reader.ReadValue<Color>(null);
                Assert.True(value == Color.Blue);
                Assert.True(reader.Consumed == 1);

                buffer.Dispose();
            }

            {
                Assert.Throws<CoPackException>(() =>
                {
                    var buffer = new PooledByteBufferWriter();
                    var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
                    writer.WriteVarInt(5);
                    var reader = new CoPackReader(buffer.WrittenSpan);
                    var value = reader.ReadValue<Color>(null);
                });
            }
        }
    }

    [Fact(DisplayName = "读写可空的值对象")]
    public void WriteReadNullable()
    {
        CoPackFormatterProvider.Register(new EnumFormatter<Color>());
        CoPackFormatterProvider.Register(new NullableFormatter<Color>());

        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        int? value1 = null;
        int? value2 = 127;
        Color? color1 = Color.White;
        Color? color2 = null;

        writer.WriteValue(value1, PackFlags.All);
        writer.WriteValue(value2, PackFlags.All);
        writer.WriteValue(color1, PackFlags.All);
        writer.WriteValue(color2, PackFlags.All);

        Assert.True(buffer.WrittenSpan.Length == 1 + 2 + 1 + 1);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b000_00000, 0b001_10001, 127,
            0b001_00000, 0b000_00000,
        }));

        var reader = new CoPackReader(buffer.WrittenSpan);
        value1 = reader.ReadValue<int?>(null);
        Assert.True(value1 == null);
        value2 = reader.ReadValue<int?>(null);
        Assert.True(value2 is 127);
        color1 = reader.ReadValue<Color?>(null);
        Assert.True(color1 is Color.White);
        color2 = reader.ReadValue<Color?>();
        Assert.True(color2 == null);
        Assert.True(reader.Consumed == 5);
        
        buffer.Dispose();
    }

    [Fact(DisplayName = "读写List")]
    public void WriteReadList()
    {
        CoPackFormatterProvider.Register(new ListFormatter<Vector2>());
        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        var vectors = new List<Vector2>()
        {
            new(0, 0),
            new(0.1234f, 0.1234f)
        };
        writer.WriteValue(vectors, PackFlags.All);


        Assert.True(buffer.WrittenSpan.Length == 15);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b011_00010, // type=list, len=2 
            0b011_00010, // type=list, len=2
            0b010_00000, // type=float, value=0
            0b010_00000, // type=float, value=0
            0b011_00010, // type=list, len=2
            0b010_00001, 0x24, 0xb9, 0xfc, 0x3d, // type=float, value=0.1234
            0b010_00001, 0x24, 0xb9, 0xfc, 0x3d, // type=float, value=0.1234
        }));
        
        var reader = new CoPackReader(buffer.WrittenSpan);
        var vectors2 = reader.ReadValue<List<Vector2>>();
        Assert.True(vectors2 != null);
        Assert.True(vectors2.Count == 2);
        Assert.Equal(vectors, vectors2);
        
        buffer.Dispose();
    }

    [Fact(DisplayName = "读写HashSet")]
    public void WriteReadHashSet()
    {
        CoPackFormatterProvider.Register(new HashSetFormatter<int>());
        {
            var buffer = new PooledByteBufferWriter();
            var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
            HashSet<int>? list = null;
            writer.WriteValue(list, PackFlags.All);
            Assert.True(buffer.WrittenSpan.Length == 1);
            Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[] {0b000_00000}));
            
            var reader = new CoPackReader(buffer.WrittenSpan);
            var values = reader.ReadValue<HashSet<int>>();
            Assert.True(values == null);
        }

        {
            Assert.Throws<CoPackException>(() =>
            {
                // 测试没有注册formatter抛异常的情况
                var buffer = new PooledByteBufferWriter();
                var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);
                HashSet<long>? list = null;
                writer.WriteValue(list, PackFlags.All);
            });
        }
    }

    [Fact(DisplayName = "读写Dict")]
    public void WriteReadDictionary()
    {
        CoPackFormatterProvider.Register(new DictionaryFormatter<int, bool>());

        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        Dictionary<int, bool> dict = new()
        {
            [1] = true,
            [0xFFFF] = false,
        };
        writer.WriteValue(dict, PackFlags.All);

        Assert.True(buffer.WrittenSpan.Length == 7);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b100_00011, // M: type=map, len=2
            0b001_00001, // K: type=int, value=1
            0b000_00010, // V: type=bool, value=true 
            0b001_10011, 0xFF, 0xFF, // K: type=int, value=0xFFFF
            0b000_00001, // V: type=bool, value=false;
        }));
        
        var reader = new CoPackReader(buffer.WrittenSpan);
        var value = reader.ReadValue<Dictionary<int, bool>>();
        Assert.True(value != null);
        Assert.True(value.Count == 2);
        Assert.True(value[1] == true);
        Assert.True(value[0xFFFF] == false);
    }

    [Fact(DisplayName = "读写Object")]
    public void WriteReadObject()
    {
        CoPackFormatterProvider.Register(new MyObjectFormatter());

        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        var obj = new MyObject()
        {
            Field1 = false, // 默认值不存
            Field2 = 0x7FFFFFFF,
        };
        writer.WriteValue(obj, PackFlags.All);

        Assert.True(buffer.WrittenSpan.Length == 8);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b100_00000, // M: type=object
            0b001_00001, // K: type=int, value=1
            0b001_10100, 0xFF, 0xFF, 0xFF, 0x7F, // V: type=int, value=0x7FFFFFFF 
            0b000_00000, // Null
        }));

        var reader = new CoPackReader(buffer.WrittenSpan);
        var value = reader.ReadValue<MyObject>();
        Assert.True(value != null);
        Assert.True(value.Field1 == false);
        Assert.True(value.Field2 == 0x7FFFFFFF);
        
        _output.WriteLine(CoPackSerializer.ToJson(buffer.WrittenSpan));
    }

    [Fact(DisplayName = "读写Union")]
    public void WriteReadUnion()
    {
        CoPackFormatterProvider.Register(new BaseObjectFormatter());
        CoPackFormatterProvider.Register(new DerivedObjectFormatter());

        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        var obj = new DerivedObject()
        {
            Field1 = true,
            Field2 = true,
        };
        BaseObject baseObj = obj;

        writer.WriteValue(baseObj, PackFlags.All);
        Assert.True(buffer.WrittenSpan.Length == 7);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b101_00001, // M: type=union, tag=1
            0b100_00000, // M: type=object
            0b001_00000, // K: type=int, value=0
            0b000_00010, // V: type=bool, value=true
            0b001_00001, // K: type=int, value=1
            0b000_00010, // V: type=bool, value=true
            0b000_00000, // Null
        }));
        
        var reader = new CoPackReader(buffer.WrittenSpan);
        var value = reader.ReadValue<BaseObject>();
        Assert.True(value != null);
        Assert.True(value is DerivedObject);
        DerivedObject derivedObject = (DerivedObject) value;
        Assert.True(derivedObject.Field1 == true);
        Assert.True(derivedObject.Field2 == true);
        Assert.True(reader.Consumed == 7);
    }

    [Fact(DisplayName = "读写ValueTuple")]
    public void WriteReadValueTuple()
    {
        CoPackFormatterProvider.Register(new ValueTupleFormatter<int, bool>());

        var buffer = new PooledByteBufferWriter();
        var writer = new CoPackWriter<PooledByteBufferWriter>(ref buffer);

        var value = (0xFFFF, true);
        writer.WriteValue(value, PackFlags.All);
        Assert.True(buffer.WrittenSpan.Length == 5);
        Assert.True(buffer.WrittenSpan.SequenceEqual(new byte[]
        {
            0b011_00010, // L: type=list, len=2
            0b001_10011, 0xFF, 0xFF, // I: type=int, value=0xFFFF
            0b000_00010, // B: type=bool, value=true
        }));
        
        var reader = new CoPackReader(buffer.WrittenSpan);
        var value2 = reader.ReadValue<(int, bool)>();
        Assert.True(value2.Item1 == 0xFFFF);
        Assert.True(value2.Item2 == true);
        Assert.True(reader.Consumed == 5);
    }

    [Fact(DisplayName = "测试Json互转")]
    public void ToAndFromJson()
    {
        CoPackFormatterProvider.Register(new MyObject2Formatter());
        CoPackFormatterProvider.Register(new ListFormatter<long>());
        CoPackFormatterProvider.Register(new DictionaryFormatter<string, double>());
        CoPackFormatterProvider.Register(new MyObjectFormatter());
        CoPackFormatterProvider.Register(new BaseObjectFormatter());
        CoPackFormatterProvider.Register(new DerivedObjectFormatter());
        
        var buffer = new PooledByteBufferWriter();
        var obj = new MyObject2();
        CoPackSerializer.Serialize(buffer, obj);
        
        _output.WriteLine(CoPackSerializer.ToJson(buffer.WrittenSpan));
    }
}

public enum Color
{
    White,
    Blue,
    Red
};

public class MyObject
{
    public bool Field1 { get; set; }
    public int Field2 { get; set; }
}

public sealed class MyObjectFormatter : ICoPackFormatter<MyObject?>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in MyObject? value, PackFlags flags)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteObjectHeader();

        if (value.Field1)
        {
            writer.WriteVarInt(0);
            writer.WriteBool(value.Field1);
        }

        if (value.Field2 != 0)
        {
            writer.WriteVarInt(1);
            writer.WriteVarInt(value.Field2);
        }

        writer.WriteNull();
    }

    public MyObject? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        var obj = new MyObject();
        reader.ReadObjectHeader();
        while (!reader.TryReadNull())
        {
            var tag = reader.ReadInt32();
            switch (tag)
            {
                case 0:
                    obj.Field1 = reader.ReadBool();
                    break;
                case 1:
                    obj.Field2 = reader.ReadInt32();
                    break;
                default:
                    CoPackException.ThrowReadUnexpectedObjectTag(tag);
                    break;
            }
        }

        return obj;
    }
}

public class BaseObject
{
    public bool Field1 { get; set; }
}

public class DerivedObject : BaseObject
{
    public bool Field2 { get; set; }
}

public sealed class BaseObjectFormatter : ICoPackFormatter<BaseObject?>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in BaseObject? value, PackFlags flags)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value)
        {
            case DerivedObject derivedObject:
                writer.WriteUnion(1, derivedObject, flags);
                break;
            case { } thisObject:
                DoWrite(ref writer, thisObject);
                break;
            default:
                CoPackException.ThrowNotFoundInUnionType(value.GetType(), typeof(BaseObject));
                break;
        }
    }

    public BaseObject? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;

        if (reader.TryReadObjectHeader())
        {
            return DoRead(ref reader, state);
        }
        
        var tag = reader.ReadUnionHeader();
        switch (tag)
        {
            case 1:
                return reader.ReadValue<DerivedObject>();
            default:
                CoPackException.ThrowReadUnexpectedUnionTag(tag);
                return null;
        }
    }

    private void DoWrite<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in BaseObject value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteObjectHeader();
        if (value.Field1)
        {
            writer.WriteVarInt(0);
            writer.WriteBool(value.Field1);
        }

        writer.WriteNull();
    }

    private BaseObject DoRead(ref CoPackReader reader, object? state)
    {
        var obj = new BaseObject();
        while (!reader.TryReadNull())
        {
            var tag = reader.ReadInt32();
            switch (tag)
            {
                case 0:
                    obj.Field1 = reader.ReadBool();
                    break;
                default:
                    CoPackException.ThrowReadUnexpectedObjectTag(tag);
                    break;
            }
        }

        return obj;
    }
}

public sealed class DerivedObjectFormatter : ICoPackFormatter<DerivedObject?>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in DerivedObject? value, PackFlags flags)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteObjectHeader();
        if (value.Field1)
        {
            writer.WriteVarInt(0);
            writer.WriteBool(value.Field1);
        }

        if (value.Field2)
        {
            writer.WriteVarInt(1);
            writer.WriteBool(value.Field2);
        }

        writer.WriteNull();
    }

    public DerivedObject? Read(ref CoPackReader reader, object? state)
    {
        if (reader.TryReadNull())
            return null;
        
        reader.ReadObjectHeader();
        var obj = new DerivedObject();
        while (!reader.TryReadNull())
        {
            var tag = reader.ReadInt32();
            switch (tag)
            {
                case 0:
                    obj.Field1 = reader.ReadBool();
                    break;
                case 1:
                    obj.Field2 = reader.ReadBool();
                    break;
                default:
                    CoPackException.ThrowReadUnexpectedObjectTag(tag);
                    break;
            }
        }

        return obj;
    }
}

public class MyObject2
{
    public int IntField { get; set; } = 1000;
    public bool BoolField { get; set; } = true;
    public string StringField { get; set; } = "hello 世界";
    public float FloatField { get; set; } = 1.234567f;
    public List<long> LongFields { get; set; } = [1, 2, 3];
    public Dictionary<string, double> DictField { get; set; } = new() 
    {
        {"hello", 1.0},
        {"强大", 100.0},
    };
    public MyObject ObjectField { get; set; } = new MyObject() 
    {
        Field1 = true,
        Field2 = 1000100,
    };
    public BaseObject UnionField { get; set; } = new DerivedObject() 
    {
        Field1 = true,
        Field2 = true,
    };
}

public sealed class MyObject2Formatter: ICoPackFormatter<MyObject2>
{
    public void Write<TBufferWriter>(ref CoPackWriter<TBufferWriter> writer, in MyObject2? value, PackFlags flags) 
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteObjectHeader();

        if (value.IntField != 0)
        {
            writer.WriteVarInt(0);
            writer.WriteVarInt(value.IntField);
        }

        if (value.BoolField)
        {
            writer.WriteVarInt(1);
            writer.WriteBool(value.BoolField);
        }

        writer.WriteVarInt(2);
        writer.WriteString(value.StringField);

        if (value.FloatField != 0)
        {
            writer.WriteVarInt(3);
            writer.WriteFloat(value.FloatField);
        }
        
        writer.WriteVarInt(4);
        writer.WriteValue(value.LongFields, flags);
        
        writer.WriteVarInt(5);
        writer.WriteValue(value.DictField, flags);
        
        writer.WriteVarInt(6);
        writer.WriteValue(value.ObjectField, flags);
        
        writer.WriteVarInt(7);
        writer.WriteValue(value.UnionField, flags);

        writer.WriteNull();
    }

    public MyObject2? Read(ref CoPackReader reader, object? state)
    {
        throw new NotImplementedException();
    }
}
