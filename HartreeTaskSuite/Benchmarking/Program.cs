using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TickData;
using TickData.Interfaces;
using TickData.Model;

namespace Benchmarking
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TickCreations>();
        }
    }
    /*
    
|       Method |           Mean |         Error |        StdDev |
|------------- |---------------:|--------------:|--------------:|
| CreateTicks0 |       2.839 us |     0.0632 us |     0.1825 us |
| CreateTicks1 |      10.596 us |     0.2335 us |     0.6738 us |
| CreateTicks2 |     102.061 us |     1.9624 us |     4.7018 us |
| CreateTicks3 |   1,168.931 us |    27.1740 us |    79.6967 us |
| CreateTicks4 |  13,112.255 us |   385.2869 us | 1,129.9796 us |
| CreateTicks5 | 166,790.906 us | 3,235.0902 us | 8,234.3375 us |    

     */
    public class TickCreations
    {
        string[] key0 = Enumerable.Range(1, 2).Select(i => "k" + i).ToArray();
        string[] key1 = Enumerable.Range(1, 10).Select(i => "k" + i).ToArray();
        string[] key2 = Enumerable.Range(1, 100).Select(i => "k" + i).ToArray();
        string[] key3 = Enumerable.Range(1, 1000).Select(i => "k" + i).ToArray();
        string[] key4 = Enumerable.Range(1, 10000).Select(i => "k" + i).ToArray();
        string[] key5 = Enumerable.Range(1, 100000).Select(i => "k" + i).ToArray();


        IBuildData BuildData = new TickBuilder();
        IJsonConverter<Tick> JsonConverter = new TickJsonConverter<Tick>();
        Random rand = new Random();

        [Benchmark]
        public void CreateTicks0()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key0.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
        [Benchmark]
        public void CreateTicks1()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key1.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
        [Benchmark]
        public void CreateTicks2()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key2.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
        [Benchmark]
        public void CreateTicks3()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key3.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
        [Benchmark]
        public void CreateTicks4()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key4.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
        [Benchmark]
        public void CreateTicks5()
        {
            string utcDateTime = DateTime.UtcNow.ToString();
            var data = key5.Select(k => JsonConverter.Serialize(BuildData.BuildData(k, utcDateTime, rand.NextDouble()))).ToArray();
        }
    }
}