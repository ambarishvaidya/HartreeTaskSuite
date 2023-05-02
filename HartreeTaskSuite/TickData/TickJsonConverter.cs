using System.Text.Json;
using TickData.Interfaces;

namespace TickData
{
    public class TickJsonConverter<T> : IJsonConverter<T> where T : class
    {
        public T Deserialize(string s)
        {
            return string.IsNullOrEmpty(s)
                ? null
                : JsonSerializer.Deserialize<T>(s);
        }

        public string Serialize(T t)
        {
            return JsonSerializer.Serialize<T>(t);
        }
    }
}
