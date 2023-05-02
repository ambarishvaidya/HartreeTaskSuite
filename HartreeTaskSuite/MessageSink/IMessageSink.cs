using System.Collections.Concurrent;

namespace MessageSink
{
    public interface IMessageSink<T> where T : class
    {
        ConcurrentQueue<T> MessageQueue { get; }
        AutoResetEvent Trigger { get; }
        void StartPersistingData(CancellationTokenSource cts);        
    }
}