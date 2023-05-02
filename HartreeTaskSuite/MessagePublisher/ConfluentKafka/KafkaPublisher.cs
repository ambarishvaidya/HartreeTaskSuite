using Confluent.Kafka;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace MessagePublisher.ConfluentKafka
{
    public class KafkaPublisher : IMessagePublisher, IDisposable
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _bootstrapServer;
        private string _topic;

        private ProducerConfig _producerConfig;
        private IProducer<Null, string> _producer;

        private KafkaPublisher() { }
        public KafkaPublisher(string bootstrapServer, string topic)
        {
            if (string.IsNullOrEmpty(bootstrapServer)) throw new ArgumentException(nameof(bootstrapServer) + " cannot be null or empty!");
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException(nameof(topic) + " cannnot be null or empty!");
            _bootstrapServer = bootstrapServer;
            _topic = topic;
            Connect();
        }

        public bool Connect()
        {
            if (IsConnected)
            {
                Dispose();
                _producerConfig = null;
                _producer = null;
            }

            try
            {
                _producerConfig = new ProducerConfig()
                {
                    BootstrapServers = _bootstrapServer
                };
                _producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
                IsConnected = true;

                Log.Info($"Kafka producer CONNECTED using BootstrapServers {_bootstrapServer}");
            }
            catch (Exception exception)
            {
                Log.Error($"Kafka producer NOT CONNECTED using BootstrapServers {_bootstrapServer}. " +
                    $"{Environment.NewLine}StackTrace : {exception}");
                IsConnected = false;
            }
            return IsConnected;
        }

        public static void DeliveryReportHandler(DeliveryReport<Null, string> deliveryReport)
        {
            if (deliveryReport.Error.Code != ErrorCode.NoError)
            {
                Log.Error($"FAILED PUBLISHING {deliveryReport.Value}. Reason : {deliveryReport.Error.Reason}");
            }
            else
            {
                Log.Error($"SUCCESS PUBLISHING {deliveryReport.Value}.");
            }
        }

        public void PublishMessages(string[] data)
        {
            foreach (var d in data)
                PublishMessage(d);
        }

        public void PublishMessage(string data)
        {
            try
            {
                _producer.Produce(_topic, new Message<Null, string> { Value = data }, DeliveryReportHandler);
                Log.Info($"Published {data}");
            }
            catch (Exception exception)
            {
                Log.Info($"ERROR Publising {data}. Msg : {exception.Message}");
            }
        }

        public bool IsConnected { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_producer != null)
                {
                    _producer.Dispose();
                }
            }
        }
    }
}
