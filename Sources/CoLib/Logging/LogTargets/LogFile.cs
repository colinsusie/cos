// Written by Colin on 2023-11-03

using System.Diagnostics;
using CoLib.Logging.LogTargets;

namespace CoLib.Logging.OutputTargets;

/// <summary>
/// 输出到文件
/// </summary>
public class LogFile : LogTarget
{
    public const long DefaultRollOnFileSize = 100L * 1024 * 1024; // 100MB
    private const string MacroProcessId = "{ProcessId}";
    private const string MacroProcessName = "{ProcessName}";
    private const string MacroDateTime = "{DateTime}";
    private const string MacroDate = "{Date}";
    
    // 文件超过一定大小后滚动
    private readonly long _rollOnFileSize;
    private readonly object _syncObj = new();
    private bool _isDisposed;

    private int _currFileSeq;
    private LogFileWriter? _logFile;

    private readonly string _directory;
    private readonly string _filenamePrefix;
    private readonly string _filenameSuffix;

    /// <summary>
    /// 输出日志到文件
    /// </summary>
    /// <param name="logMgr">所属日志库</param>
    /// <param name="minLogLevel">允许输出的最小等级</param>
    /// <param name="path">文件路径，文件名支持宏替换，目前支持宏有: <br/>
    ///   - {ProcessName} 进程名
    ///   - {ProcessId} 进程Id <br/>
    ///   - {DateTime} 日期格式为 yyyyMMddHHmmss <br/>
    ///   - {Date} 日期格式为 yyyyMMdd <br/>
    ///   例子: {GameServer}-{ProcessId}-{Date}.log
    /// </param>
    /// <param name="filterFunc">过滤函数，返回false将不输出日志到文件</param>
    /// <param name="rollOnFileSize">文件大小限制，如果传0表示没有限制<br/>
    ///  超过大小将创建新的日志文件，文件名在后面增加一个_N编码，比如：app.log app_1.log app_2.log
    /// </param>
    public LogFile(LogManager logMgr,
        string path,
        LogLevel minLogLevel = LogLevel.Debug,
        Func<LogItem, bool>? filterFunc = null,
        long rollOnFileSize = DefaultRollOnFileSize): base(logMgr, minLogLevel, filterFunc)
    {
        if (rollOnFileSize < 0)
        {
            throw new ArgumentException("Roll on file size must be greater than or equal to 0",
                nameof(rollOnFileSize));
        }
        
        _rollOnFileSize = rollOnFileSize;

        var pathDirectory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(pathDirectory))
            pathDirectory = Directory.GetCurrentDirectory();
        _directory = Path.GetFullPath(pathDirectory);
        _filenameSuffix = Path.GetExtension(path);

        var process = Process.GetCurrentProcess();
        var fileName = Path.GetFileNameWithoutExtension(path);
        fileName = fileName.Replace(MacroProcessName, process.ProcessName);
        fileName = fileName.Replace(MacroProcessId, process.Id.ToString());
        _filenamePrefix = fileName;
    }

    public override void Output(in LogItem logItem)
    {
        lock (_syncObj)
        {
            if (_isDisposed) throw new ObjectDisposedException("The log file has been disposed.");

            if (_logFile == null)
            {
                OpenFile();
            }
            else if (_rollOnFileSize > 0 && _logFile.FileSize >= _rollOnFileSize)
            {
                CloseFile();
                OpenFile();
            }

            // 输出目标到文件
            _logFile!.WriteLine(logItem.Message);
        }
    }

    public override void Dispose()
    {
        lock (_syncObj)
        {
            CloseFile();
            _isDisposed = true;
        }
    }

    private void CloseFile()
    {
        if (_logFile != null)
        {
            _logFile.Dispose();
            _logFile = null;
        }
    }

    private void OpenFile()
    {
        var now = DateTime.Now;
        var filePrefix = _filenamePrefix.Replace(MacroDate, now.ToString("yyyyMMdd"));
        filePrefix = filePrefix.Replace(MacroDateTime, now.ToString("yyyyMMddHHmmss"));

        while (true)
        {
            var fileNo = _currFileSeq == 0 ? "" : $"_{_currFileSeq:000}";
            var filePath = Path.Combine(_directory, $"{filePrefix}{fileNo}{_filenameSuffix}");
            _currFileSeq++;

            if (File.Exists(filePath))
                continue;
            
            try
            {
                _logFile = new LogFileWriter(filePath);
            }
            catch (IOException)
            {
                continue;
            }

            break;
        }
    }
}