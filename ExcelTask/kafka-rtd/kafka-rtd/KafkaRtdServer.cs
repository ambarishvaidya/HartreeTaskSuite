using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NLog;
using Confluent.Kafka;
//using Confluent.Kafka.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace kafka_rtd
{
    [
        Guid("653ED879-32B3-469D-BE74-831652D78919"),
        // This is the string that names RTD server.
        // Users will use it from Excel: =RTD("kafka",, ....)
        ProgId("kafka")
    ]
    public class KafkaRtdServer : IRtdServer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        IRtdUpdateEvent _callback;
        ILogger Log = LogManager.GetCurrentClassLogger();

        //DispatcherTimer _timer;
        SubscriptionManager _subMgr;
        bool _isExcelNotifiedOfUpdates = false;
        object _notifyLock = new object();
        object _syncLock = new object();
        int _lastRtdTopic = -1;
        
        //private Dictionary<Uri, Consumer<Null, string>> _kafkaConsumers = new Dictionary<Uri, Consumer<Null, string>>();

        private const string LAST_RTD = "LAST_RTD";
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public KafkaRtdServer()
        {
        }
        // Excel calls this. It's an entry point. It passes us a callback
        // structure which we save for later.
        int IRtdServer.ServerStart (IRtdUpdateEvent callback)
        {
            _callback = callback;
            _subMgr = new SubscriptionManager(() => {
                try
                {
                    if (_callback != null)
                    {
                        if (!_isExcelNotifiedOfUpdates)
                        {
                            lock (_notifyLock)
                            {
                                _callback.UpdateNotify();
                                _isExcelNotifiedOfUpdates = true;
                            }
                        }
                    }
                } catch(Exception ex)  // HRESULT: 0x8001010A (RPC_E_SERVERCALL_RETRYLATER
                {
                    Console.WriteLine(ex.Message);
                }
            });
            return 1;
        }

        // Excel calls this when it wants to shut down RTD server.
        void IRtdServer.ServerTerminate ()
        {
            _cancellationTokenSource.Cancel();
        }
        // Excel calls this when it wants to make a new topic subscription.
        // topicId becomes the key representing the subscription.
        // String array contains any aux data user provides to RTD macro.
        object IRtdServer.ConnectData (int topicId, ref Array strings, ref bool newValues)
        {
            try
            {
                newValues = true;

                if (strings.Length == 1)
                {
                    string host = strings.GetValue(0).ToString().ToUpperInvariant();

                    switch (host)
                    {
                        case LAST_RTD:
                            _lastRtdTopic = topicId;
                            return DateTime.Now.ToLocalTime();
                    }
                    return "ERROR: Expected: LAST_RTD or host, topic, field";
                }
                else if (strings.Length >= 2)
                {
                    // Crappy COM-style arrays...
                    string host = strings.GetValue(0).ToString();
                    string topic = strings.GetValue(1).ToString();
                    string field = strings.Length > 2 ? strings.GetValue(2).ToString() : "";

                    return Subscribe(topicId, host, topic, field);
                }
                return "ERROR: Expected: LAST_RTD or host, topic, field";

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ex.Message;
            }
        }

        private object Subscribe(int topicId, string host, string topic, string field)
        {
            if (topic != "test") return new object();
            try
            {
                if (String.IsNullOrEmpty(topic))
                    return "<topic required>";

                Uri hostUri = new Uri(host);
                if (_subMgr.Subscribe(topicId, hostUri, topic, field))
                    return _subMgr.GetValue(topicId); // already subscribed 

                var rtdSubTopic = SubscriptionManager.FormatPath(host, topic);

                Task.Run(() =>
                {
                    var conf = new Dictionary<string, string>
                            {
                                { "group.id", "test-consumer-group" },
                                { "bootstrap.servers", host },
                                //{ "metadata.max.age.ms", 10000 },
                                { "auto.commit.interval.ms", "5000" },
                                { "auto.offset.reset", "latest" }
                            }.ToList().AsEnumerable();
                    using(var consumer = new ConsumerBuilder<string, string>(conf).Build())
                    {
                        consumer.Subscribe(topic);
                        try
                        {
                            while(true)
                            {
                                try
                                {
                                    var cr = consumer.Consume(_cancellationTokenSource.Token);
                                    string msg = cr.Message.Value;                                    
                                    Tick data = JsonConvert.DeserializeObject<Tick>(cr.Message.Value);
                                    var rtdTopicString = SubscriptionManager.FormatPath(host, topic, data.key);
                                    _subMgr.Set(rtdTopicString, data.value.value.ToString());
                                }
                                catch (Exception incorrectmessage)
                                {
                                    string err = incorrectmessage.Message;
                                }
                            }
                        }
                        catch(OperationCanceledException oex)
                        {

                        }
                        catch(Exception oex)
                        {

                        }
                    }                    
                });
                return SubscriptionManager.UninitializedValue;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                _subMgr.Set(topicId, ex.Message);
            }
            return _subMgr.GetValue(topicId);
        }
        
        // Excel calls this when it wants to cancel subscription.
        void IRtdServer.DisconnectData (int topicId)
        {
            //SubscriptionManager.SubInfo sub = _subMgr.GetSub(topicId);
            //Uri hostUri = new Uri(sub.Host);
            //if (_kafkaConsumers.TryGetValue(hostUri, out Consumer<Null, string> consumer))
            //{
            //    //if (consumer.count<=0)
            //    _kafkaConsumers.Remove(hostUri);
            //    consumer.Dispose();
            //}
            _subMgr.Unsubscribe(topicId);
        }
        // Excel calls this every once in a while.
        int IRtdServer.Heartbeat ()
        {
            lock (_notifyLock)  
                _isExcelNotifiedOfUpdates = false;  // just in case it gets stuck

            return 1;
        }
        // Excel calls this to get changed values. 
        Array IRtdServer.RefreshData (ref int topicCount)
        {
            try
            {
                List<SubscriptionManager.UpdatedValue> updates;
                lock (_syncLock)
                {
                    updates = _subMgr.GetUpdatedValues();
                }
                topicCount = updates.Count + (_lastRtdTopic < 0 ? 0 : 1);

                object[,] data = new object[2, topicCount];
                int i = 0;
                if (_lastRtdTopic > 0)
                {
                    data[0, i] = _lastRtdTopic;
                    data[1, i] = DateTime.Now.ToLocalTime();
                    i++;
                }

                foreach (var info in updates)
                {
                    data[0, i] = info.TopicId;
                    data[1, i] = info.Value;

                    i++;
                }
                return data;
            } 
            finally
            {
                lock (_notifyLock)
                    _isExcelNotifiedOfUpdates = false;
            }
        }
    }
}
