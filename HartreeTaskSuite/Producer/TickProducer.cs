using MessagePublisher;
using System.Reflection;
using System.Timers;
using TickData.Interfaces;
using TickData.Model;

namespace Producer
{
    public class TickProducer : IProducer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _tickFrequency;
        private int _tickCount;
        private IBuildData _buildData;
        private IJsonConverter<Tick> _jsonConverter;
        private IMessagePublisher _messagePublisher;
        private System.Timers.Timer _timer;
        private Random _random;

        private string[] _keys;

        private TickProducer() { }
        public TickProducer(int tickFrequency, int tickCount, IBuildData buildData, IJsonConverter<Tick> jsonConverter, IMessagePublisher messagePublisher)
        {
            _tickFrequency = tickFrequency;
            _tickCount = tickCount;
            _buildData = buildData;
            _jsonConverter = jsonConverter;
            _messagePublisher = messagePublisher;
            _random = new Random();
            Validate();
            Setup();
        }

        private void Validate()
        {
            if (_tickFrequency <= 0 || _tickFrequency > 10)
            {
                string msg = $"TICK FREQUENCY has to be between 1 - 10 both inclusive";
                Log.Error(msg);
                throw new ArgumentException(msg);
            }
            if (_tickCount <= 0 || _tickCount > 100)
            {
                string msg = $"TICK COUNT has to be between 1 - 100 both inclusive";
                Log.Error(msg);
                throw new ArgumentException(msg);
            }
            if (_buildData == null || _jsonConverter == null || _messagePublisher == null)
            {
                string msg = $"PROVIDE VALUES for all IBuildData, IJsonConverter<Tick>, IMessagePublisher. NULL not allowed.";
                Log.Error(msg);
                throw new ArgumentException(msg);
            }
        }

        private void GenerateKeys()
        {
            _keys = new string[_tickCount];
            for (int i = 0; i < _tickCount; i++)
            {
                _keys[i] = "key" + (i + 1);
            }
        }

        private void Setup()
        {
            GenerateKeys();

            _timer = new System.Timers.Timer(_tickFrequency * 1000);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DateTime utcNow = DateTime.UtcNow;
            string utcNowString = utcNow.ToString();

            var ticks = _keys.Select(k => _jsonConverter.Serialize(_buildData.BuildData(k, utcNowString, _random.NextDouble()))).ToArray();
            Publish(ticks);
        }

        public void Publish(string[] data)
        {
            _messagePublisher.PublishMessages(data);
        }
        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
