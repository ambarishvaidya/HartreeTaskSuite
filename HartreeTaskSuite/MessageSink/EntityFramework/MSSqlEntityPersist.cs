using log4net;
using MessageSink.EntityFramework.Data;
using MessageSink.EntityFramework.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Reflection;

namespace MessageSink.EntityFramework
{
    public class MSSqlEntityPersist<T> : IMessageSink<T> where T : class
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _queryLimit;
        private string _connectionString;
        private string _tableName;
        private ITickRepo _tickRepo;

        private ConcurrentQueue<T> _messageQueue;
        private AutoResetEvent _trigger;
        private CancellationTokenSource _cts;

        public MSSqlEntityPersist(string connectionstring, string tablename, int querylimit = 10) 
        {
            _connectionString = connectionstring;
            _tableName = tablename;
            _queryLimit = querylimit;

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder<MSSqlDbContext>();
            builder.UseSqlServer(connectionstring);
            MSSqlDbContext context = new MSSqlDbContext(builder.Options);
            _tickRepo = new TickRepo(context);

            _messageQueue = new ConcurrentQueue<T>();
            _trigger = new AutoResetEvent(false);                        
        }

        public ConcurrentQueue<T> MessageQueue => _messageQueue;

        public AutoResetEvent Trigger => _trigger;

        public void StartPersistingData(CancellationTokenSource cts)
        {
            Log.Info($"Starting monitoring MessageQueue for messages to Persist");
            _cts = cts;
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    _trigger.WaitOne();
                    int counter = 0;
                    List<Tick> tickList = new List<Tick>();
                    while (MessageQueue.TryDequeue(out T result) && counter < _queryLimit && !_cts.IsCancellationRequested)
                    {
                        TickData.Model.Tick data = result as TickData.Model.Tick;
                        tickList.Add(new Tick() { TickKey = data.key, TickTimeInUtc = DateTime.Parse(data.value.time), TickValue = data.value.value });
                        
                        counter++;
                    }
                    if (counter >= _queryLimit)
                        _trigger.Set();


                    if (!tickList.Any()) continue;
                    await _tickRepo.SaveTicks(tickList.ToArray());
                    tickList.Clear();
                    tickList = null;                    
                }
            });
        }
    }
}
