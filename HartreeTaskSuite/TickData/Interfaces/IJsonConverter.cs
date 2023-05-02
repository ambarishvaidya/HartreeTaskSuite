namespace TickData.Interfaces
{
    public interface IJsonConverter<T> where T : class
    {
        string Serialize(T t);
        T Deserialize(string s);
    }
}
