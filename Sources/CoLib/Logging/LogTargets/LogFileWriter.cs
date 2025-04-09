// Written by Colin on 2023-11-05

using System.Text;

namespace CoLib.Logging.OutputTargets;

/// <summary>
/// 日志文件输出
/// </summary>
public sealed class LogFileWriter: IDisposable
{
    private readonly TextWriter _output;
    private readonly StreamWrapper _streamWrapper;

    public LogFileWriter(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Stream outputStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        outputStream.Seek(0, SeekOrigin.End);
        _streamWrapper = new StreamWrapper(outputStream);
        
        _output = new StreamWriter(_streamWrapper, new UTF8Encoding(false));
    }

    public void WriteLine(string message)
    {
        _output.WriteLine(message);
        _output.Flush();
    }

    public long FileSize => _streamWrapper.StreamSize;
    
    public void Dispose()
    {
        _output.Dispose();
    }
}

internal sealed class StreamWrapper : Stream
{
    private readonly Stream _stream;

    public StreamWrapper(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        StreamSize = stream.Length;
    }

    public long StreamSize { get; private set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();

        base.Dispose(disposing);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
        StreamSize += count;
    }

    public override void Flush() => _stream.Flush();
    public override bool CanRead => false;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => true;
    public override long Length => _stream.Length;


    public override long Position
    {
        get => _stream.Position;
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new InvalidOperationException($"Seek operations are not available through `{nameof(StreamWrapper)}`.");
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);

        if (value < StreamSize)
        {
            StreamSize = _stream.Length;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}