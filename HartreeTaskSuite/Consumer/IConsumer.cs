namespace Consumer
{
    public interface IConsumer<T> where T : class
    {
        void Start();
        void Stop();
    }
}
