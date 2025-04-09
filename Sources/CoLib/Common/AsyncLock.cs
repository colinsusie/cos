// Written by Colin on 2024-8-4

namespace CoLib.Common;

/// <summary>
/// 异步互斥锁
/// </summary>
public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1);

    public async Task<AsyncLockDisposer> AcquireAsync()
    {
        await _semaphore.WaitAsync();
        return new AsyncLockDisposer(this);
    }

    public readonly struct AsyncLockDisposer : IDisposable
    {
        private readonly AsyncLock _asyncLock;

        public AsyncLockDisposer(AsyncLock asyncLock)
        {
            _asyncLock = asyncLock;
        }
        
        public void Dispose()
        {
            _asyncLock._semaphore.Release();
        }
    }
}