namespace Orchestrator.Utils;

public struct ScopeReadLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ScopeReadLock(ReaderWriterLockSlim rwLock)
    {
        _lock = rwLock;
        _lock.EnterReadLock();    
    }
    
    public void Dispose()
    {
        _lock.ExitReadLock();
    }
}

public struct ScopeWriteLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ScopeWriteLock(ReaderWriterLockSlim rwLock)
    {
        _lock = rwLock;
        _lock.EnterWriteLock();    
    }
    
    public void Dispose()
    {
        _lock.ExitWriteLock();
    }
}
