namespace Producer
{
    public interface IProducer
    {
        void Start();
        void Stop();
        void Publish(string[] messages);
    }
}
