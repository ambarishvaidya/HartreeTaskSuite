namespace MessagePublisher
{
    public interface IMessagePublisher
    {
        bool Connect();
        bool IsConnected { get; }
        void PublishMessages(string[] messages);
        void PublishMessage(string message);
    }
}