using System.Configuration;
using System.Reflection;
using TickData.Interfaces;
using TickData.Model;
using TickData;
using MessageConsumer;
using MessageConsumer.ConfluentKafka;
using MessageSink;
using MessageSink.BulkQuery;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Consumer
{
    public class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _tickFrequency;
        private string _bootstrapServer;
        private string _groupId;
        private string _offset;
        private string _topicName;

        private IMessageConsumer<Tick> _kafkaConsumer;
        private IJsonConverter<Tick> _jsonConverter;
        private IConsumer<Tick> _consumer;
        IMessageSink<Tick> _messageSink;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            new Program();
        }

        public Program()
        {
            Log.Info($"---------------------- Starting Producer --------------------------------");
            ReadConfiguration();
            Initialize();
            RunConsumer();
        }

        private void RunConsumer()
        {
            Console.WriteLine("Press Ctrl + C to stop the Producer!");            
            Console.CancelKeyPress += (s, e) =>
            {
                Log.Info("User Requested Cancel of Producer");
                _consumer.Stop();
            };           

            _consumer.Start();
            Console.ReadLine();
        }

        private void Initialize()
        {
            string msg = $"Initializing -----------------------{Environment.NewLine}";
            try
            {
                msg += $"{"":20}Creating TickJsonConverter {Environment.NewLine}";
                _jsonConverter = new TickJsonConverter<Tick>();

                msg += $"{"":20}Creating KafkaConsumer<Tick> {Environment.NewLine}";
                _kafkaConsumer = new KafkaConsumer<Tick>(_bootstrapServer, _groupId, _offset, _topicName, _jsonConverter);

                msg += $"{"":20}Creating MSSqlBulkQuery {Environment.NewLine}";
                _messageSink = new MSSqlBulkQuery<Tick>(@"Server=tcp:hartree.database.windows.net,1433;Database=Hartree;Uid=avv2;Pwd=Password123$;Encrypt=yes;TrustServerCertificate=no;",
                "DataDump");

                msg += $"{"":20}Creating TickConsumer {Environment.NewLine}";
                _consumer = new TickConsumer<Tick>(_tickFrequency, _kafkaConsumer, _messageSink);

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

                
                _bootstrapServer = ConfigurationManager.AppSettings["bootstrapServer"];
                msg += $"{_bootstrapServer,20} as BootstrapServer{Environment.NewLine}";

                _topicName = ConfigurationManager.AppSettings["topicname"];
                msg += $"{_topicName,20} as TopicName{Environment.NewLine}";

                _groupId = ConfigurationManager.AppSettings["groupid"];
                msg += $"{_groupId,20} as TopicName{Environment.NewLine}";

                _offset = ConfigurationManager.AppSettings["offset"];
                msg += $"{_offset,20} as TopicName{Environment.NewLine}";

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