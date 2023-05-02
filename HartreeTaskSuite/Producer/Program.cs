using MessagePublisher;
using MessagePublisher.ConfluentKafka;
using System;
using System.Configuration;
using System.Reflection;
using TickData;
using TickData.Interfaces;
using TickData.Model;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Producer
{
    public class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _tickFrequency;
        private int _tickCount;
        private string _bootstrapServer;
        private string _topicName;

        private IBuildData _buildData;
        private IJsonConverter<Tick> _jsonConverter;
        private IMessagePublisher _messagePublisher;
        private IProducer _producer;

        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            Log.Info($"---------------------- Starting Producer --------------------------------");
            ReadConfiguration();
            Initialize();
            RunProducer();
        }

        private void RunProducer()
        {
            Console.WriteLine("Press Ctrl + C to stop the Producer!");
            Console.CancelKeyPress += (s, e) => 
            {
                Log.Info("User Requested Cancel of Producer");
                _producer.Stop();
            };

            Log.Info("Starting Producer");
            _producer.Start();
            Console.ReadLine();
        }

        private void Initialize()
        {
            string msg = $"Initializing -----------------------{Environment.NewLine}";
            try
            {
                msg += $"{"":20}Creating TickBuilder {Environment.NewLine}";
                _buildData = new TickBuilder();

                msg += $"{"":20}Creating TickJsonConverter {Environment.NewLine}";
                _jsonConverter = new TickJsonConverter<Tick>();

                msg += $"{"":20}Creating KafkaPublisher {Environment.NewLine}";
                _messagePublisher = new KafkaPublisher(_bootstrapServer, _topicName);

                msg += $"{"":20}Creating TickProducer {Environment.NewLine}";
                _producer = new TickProducer(_tickFrequency, _tickCount, _buildData, _jsonConverter, _messagePublisher);

                msg += "Initialised!";
            }
            catch (Exception exception)
            {
                msg += "ERROR IN Initializing!";
                Log.Error($"Error in Initialization. Application will Exit. Msg : {exception.Message}");
                throw;
            }
            finally
            {                
                Log.Info(msg);
            }
        }

        private void ReadConfiguration()
        {
            string msg = $"Reading Configuration-----------------------{Environment.NewLine}";
            try
            {
                _tickFrequency = int.Parse(ConfigurationManager.AppSettings["tickfrequency"]);
                msg += $"{_tickFrequency,20} as TickFrequency{Environment.NewLine}"; 

                _tickCount = int.Parse(ConfigurationManager.AppSettings["tickcount"]);
                msg += $"{_tickCount,20} as TickCount{Environment.NewLine}";

                _bootstrapServer = ConfigurationManager.AppSettings["bootstrapServer"];
                msg += $"{_bootstrapServer,20} as BootstrapServer{Environment.NewLine}";

                _topicName = ConfigurationManager.AppSettings["topicname"];
                msg += $"{_topicName,20} as TopicName{Environment.NewLine}";

                msg += "Configuration Read!";
            }
            catch (Exception exception)
            {
                msg += "ERROR in Reading Configuration!";
                Log.Error($"INVALID configuration. Application will Exit. Msg : {exception.Message}");
                throw;
            }
            finally
            {
                
                Log.Info(msg);
            }
        }
    }
}