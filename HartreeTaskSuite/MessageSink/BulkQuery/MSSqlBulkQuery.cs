using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Reflection;
using TickData.Model;

namespace MessageSink.BulkQuery
{
    public class MSSqlBulkQuery<T> : IMessageSink<T> where T : class
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _connectionString;
        private string _tableName;

        private ConcurrentQueue<T> _messageQueue;
        private AutoResetEvent _trigger;
        private CancellationTokenSource _cts;
        private string _sql = "INSERT INTO {0} ([key], time, value) VALUES ( '{1}', '{2}', {3});";

        public MSSqlBulkQuery(string connectonstring, string tablename)
        {
            _connectionString = connectonstring;
            _tableName = tablename;

            Validate();

            _messageQueue = new ConcurrentQueue<T>();
            _trigger = new AutoResetEvent(false);
        }

        private void Validate()
        {
            if (!string.IsNullOrEmpty(_connectionString) && !string.IsNullOrEmpty(_tableName)) return;
            string msg = $"Connectionstring and tablename are mandatory fields.";
            Log.Error(msg);
            throw new ArgumentException(msg);
        }

        public void StartPersistingData(CancellationTokenSource cts)
        {
            Log.Info($"Starting monitoring MessageQueue for messages to Persist");
            _cts = cts;
            Task.Run(async () =>
            { 
                while(!_cts.IsCancellationRequested)
                {
                    _trigger.WaitOne();
                    int counter = 0;
                    string longQuery = "";
                    while (MessageQueue.TryDequeue(out T result) && counter < 10 && !_cts.IsCancellationRequested)
                    {
                        Tick data = result as Tick;
                        longQuery += string.Format(_sql, _tableName, data.key, data.value.time, data.value.value);
                        counter++;
                    }
                    if(counter >= 10)                    
                        _trigger.Set();

                    
                    if (string.IsNullOrEmpty(longQuery)) continue;
                    await SaveToDatabaseAsync(longQuery);
                    //SaveToDatabaseAsync(longQuery);
                }
            });            
        }

        public async Task<int> SaveToDatabaseAsync(string longQuery)
        //public int SaveToDatabaseAsync(string longQuery)
        {
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(longQuery, connection);
                    var result = await cmd.ExecuteNonQueryAsync();
                    //var result = cmd.ExecuteNonQuery();
                    Log.Info($"Saved data to database. {Environment.NewLine}{string.Join(Environment.NewLine, longQuery.Split(";"))}");
                    return result;
                }
                catch (Exception ex)
                {
                    Log.Error($"ERROR Saving data to database. {Environment.NewLine}{string.Join(Environment.NewLine, longQuery.Split(";"))} {ex.StackTrace}");
                    return -1;
                }
                //finally
                //{
                //    connection.Close();
                //}
            }
        }

        public ConcurrentQueue<T> MessageQueue => _messageQueue;

        public AutoResetEvent Trigger => _trigger;
    }
}
