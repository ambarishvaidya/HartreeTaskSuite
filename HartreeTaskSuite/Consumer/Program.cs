using System.Configuration;
using System.Reflection;
using TickData.Interfaces;
using TickData.Model;
using TickData;
using MessageConsumer;
using MessageConsumer.ConfluentKafka;
using MessageSink;
using MessageSink.BulkQuery;
using MessageSink.EntityFramework;

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
        private string _connectionString;
        private string _tableName;
        private string _sink;

        private IMessageConsumer<Tick> _kafkaConsumer;
        private IJsonConverter<Tick> _jsonConverter;
        private IConsumer<Tick> _consumer;
        IMessageSink<Tick> _querySink, _entitySink;

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
                _querySink = new MSSqlBulkQuery<Tick>(_connectionString, _tableName, 100);

                msg += $"{"":20}Creating MSSqlEntityPersist {Environment.NewLine}";
                _entitySink = new MSSqlEntityPersist<Tick>(_connectionString, _tableName, 100);

                msg += $"{"":20}Creating TickConsumer {Environment.NewLine}";
                _consumer = new TickConsumer<Tick>(_tickFrequency, _kafkaConsumer, _sink.ToUpper().Trim().Equals("ENTITY") ? _entitySink : _querySink);

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

                _connectionString = ConfigurationManager.AppSettings["connectionstring"];
                msg += $"{_connectionString,20} as ConnectionString{Environment.NewLine}";

                _tableName = ConfigurationManager.AppSettings["tablename"];
                msg += $"{_tableName,20} as TableName{Environment.NewLine}";

                _sink = ConfigurationManager.AppSettings["sink"];
                msg += $"{_tableName,20} as _sink{Environment.NewLine}";


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