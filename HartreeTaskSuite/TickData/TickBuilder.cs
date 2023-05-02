using TickData.Interfaces;
using TickData.Model;

namespace TickData
{
    public class TickBuilder : IBuildData
    {
        public Tick BuildData(string key, string utcDateTime, double value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(utcDateTime))
                return null;
            Tick tick = new Tick();
            tick.key = key;
            Tick.TickData tickData = new Tick.TickData();
            tickData.time = utcDateTime;
            tickData.value = value;
            tick.value = tickData;
            return tick;
        }
    }
}
