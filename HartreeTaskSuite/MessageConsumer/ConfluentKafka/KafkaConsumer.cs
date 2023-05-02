using Confluent.Kafka;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;
using TickData.Interfaces;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace MessageConsumer.ConfluentKafka
{
    public class KafkaConsumer<T> : IMessageConsumer<T> where T : class
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _bootstrapServer;
        private string _groupId;
        private string _offset;
        private string _topicName;
        private IJsonConverter<T> _jsonConverter;

        private ConcurrentQueue<T> _messageQueue;
        private ConcurrentQueue<string> _kafkaData;

        private bool _subscribed;
        private CancellationTokenSource _cts;
        private AutoResetEvent _are;

        public KafkaConsumer(string bootstrapserver, string groupid, string offset, string topicname, IJsonConverter<T> jsonconverter)
        {
            _bootstrapServer = bootstrapserver;
            _groupId = groupid;
            _offset = offset;
            _topicName = topicname;
            _jsonConverter = jsonconverter;

            Validated();

            _messageQueue = new ConcurrentQueue<T>();         
            _kafkaData = new ConcurrentQueue<string>();
        }

        public bool IsSubscribed => _subscribed;
        public void StartSubscription(CancellationTokenSource cts)
        {
            if (_subscribed) return;

            _cts = cts;
            _are = new AutoResetEvent(false);
            AddTickDataToQueue();

            Task.Run(() =>
            {                
                Dictionary<string, string> config = new Dictionary<string, string>()
                {
                    { "bootstrap.servers", _bootstrapServer },
                    { "group.id", _groupId },
                    { "auto.offset.reset", _offset }
                };
                using (var consumer = new ConsumerBuilder<string, string>(config.AsEnumerable()).Build())
                {
                    consumer.Subscribe(_topicName);
                    try
                    {
                        while (true)
                        {
                            var cr = consumer.Consume(_cts.Token);
                            Log.Debug($"Message from Kafka {cr.Message.Value}");
                            _kafkaData.Enqueue(cr.Message.Value);
                            _are.Set();
                        }
                    }
                    catch (OperationCanceledException oce)
                    {
                        Log.Info($"Subscription CANCELLED by User. Ex : {oce}");
                    }
                    catch (Exception ex)
                    {
                        Log.Info($"Subscription loop EXIT due to exception. Ex : {ex}");
                    }
                    finally
                    {
                        _subscribed = false;
                        _are.Set();
                        consumer.Close();
                    }
                }
            });
        }

        private void AddTickDataToQueue()
        {
            Task.Run(() =>
            {
                while(true && !_cts.Token.IsCancellationRequested)
                {
                    _are.WaitOne();                    
                    while(_kafkaData.TryDequeue(out string kdata) && !_cts.Token.IsCancellationRequested)
                    {                        
                        T t = _jsonConverter.Deserialize(kdata);
                        Log.Debug($"Enqueing Data {t}");
                        _messageQueue.Enqueue(t);
                    }                    
                }
                Log.Info("Stopping adding Tick data to MessageQueue");
            });
        }

        public void StopSubscription()
        {
            Log.Info("Request to STOP SUBSCRIPTION.");
            _cts.Cancel();
        }

        public ConcurrentQueue<T> MessageQueue { get { return _messageQueue; } }

        private void Validated()
        {
            Validate(_bootstrapServer, "BootstrapServer");
            Validate(_groupId, "GroupId");
            Validate(_offset, "Offset");
            Validate(_topicName, "TopicName");
            if (_jsonConverter != null) return;
            string msg = $"IJsonConverter cannot be null.";
            Log.Error(msg);
            throw new ArgumentException(msg);
        }

        private void Validate(string field, string name)
        {
            if (!string.IsNullOrEmpty(field)) return;
            string msg = $"{name} cannot be null or empty!";
            Log.Error(msg);
            throw new ArgumentException(msg);
        }
    }
}
