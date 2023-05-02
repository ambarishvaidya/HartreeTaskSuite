using TickData.Model;

namespace TickData.Interfaces
{
    public interface IBuildData
    {
        Tick BuildData(string key, string utcDateTime, double value);
    }
}
