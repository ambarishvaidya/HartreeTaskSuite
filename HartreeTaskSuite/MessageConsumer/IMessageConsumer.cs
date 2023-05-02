using System.Collections.Concurrent;

namespace MessageConsumer
{
    public interface IMessageConsumer<T> where T : class
    {
        ConcurrentQueue<T> MessageQueue { get; }
        void StartSubscription(CancellationTokenSource cts);
        void StopSubscription();
        bool IsSubscribed { get; }
    }
}