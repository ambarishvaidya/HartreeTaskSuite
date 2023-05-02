using MessageConsumer;
using System.Reflection;
using TickData.Interfaces;

namespace Consumer
{
    public class TickConsumer<T> : IConsumer<T> where T : class
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private int _tickFrequency;
        private IMessageConsumer<T> _kafkaConsumer;

        private CancellationTokenSource _cts;
        private System.Timers.Timer _timer;
        private AutoResetEvent _are;

        public TickConsumer(int tickFrequency, IMessageConsumer<T> kafkaConsumer)
        {
            _tickFrequency = tickFrequency;
            _kafkaConsumer = kafkaConsumer;
            Validated();
            _cts = new CancellationTokenSource();
        }

        private void Validated()
        {
            if (_tickFrequency <= 0 || _tickFrequency > 10)
            {
                string msg = $"TICK FREQUENCY has to be between 1 - 10 both inclusive";
                Log.Error(msg);
                throw new ArgumentException(msg);
            }
            if (_kafkaConsumer == null )
            {
                string msg = $"PROVIDE VALUES for IMessageConsumer. NULL not allowed.";
                Log.Error(msg);
                throw new ArgumentException(msg);
            }
        }

        public void Start()
        {
            _are = new AutoResetEvent(false);
            ReadMessagesFromConsumer();
            _timer = new System.Timers.Timer((_tickFrequency * 1000)/2);
            _timer.Elapsed += _timer_Elapsed;
            _kafkaConsumer.StartSubscription(_cts);
            _timer.Start();
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _are.Set();
        }

        public void Stop()
        {
            _cts.Cancel();
            _are.Set();
            if (_kafkaConsumer.IsSubscribed)
            {
                _kafkaConsumer.StopSubscription();
            }
            _timer.Stop();
        }

        private void ReadMessagesFromConsumer()
        {
            Task.Run(() =>
            {
                while (true && !_cts.IsCancellationRequested)
                {
                    _are.WaitOne();
                    _timer.Stop();
                    while (_kafkaConsumer.MessageQueue.TryDequeue(out T result) && !_cts.IsCancellationRequested)
                    {
                        Log.Debug($"Dequeued from Consumer {result}");
                    }
                    _timer.Start();
                }
            });
        }
    }
}
