using System;

namespace kafka_rtd
{
    public class Tick : IEquatable<Tick>
    {
        public string key { get; set; }
        public TickData value { get; set; }

        public class TickData
        {
            public string time { get; set; }
            public double value { get; set; }
        }

        public override string ToString()
        {
            return $"Tick[{key} : {value.time} {value.value}]";
        }

        public override bool Equals(object obj)
        {
            return (obj is Tick) ? Equals((Tick)obj) : false;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode() + value.time.GetHashCode() + value.value.GetHashCode();
        }

        public bool Equals(Tick other)
        {
            return
                other is null
                ? false
                : other.key == key
                && other.value.time == value.time
                && (other.value.value.CompareTo(value.value) == 0);
        }

        public static bool operator ==(Tick tick1, Tick tick2)
        {
            return tick1.Equals(tick2);
        }

        public static bool operator !=(Tick tick1, Tick tick2)
        {
            return !tick1.Equals(tick2);
        }
    }
}
