using MessagePublisher;
using Producer;
using System.Collections.Concurrent;
using TickData;
using TickData.Interfaces;
using TickData.Model;

namespace TestSuiteApps
{
    public class TestTickProducer
    {
        IMessagePublisher _publisher;
        IBuildData _buildData;
        IJsonConverter<Tick> _jsonConverter;

        [SetUp]
        public void SetUp()
        {
            _publisher = new MockKafkaPublisher();
            _buildData = new TickBuilder();
            _jsonConverter = new TickJsonConverter<Tick>();
        }

        [Test]
        public void TickProducer_Create_IsNotNull()
        {
            TickProducer producer = new TickProducer(tickFrequency: 1, tickCount: 2, _buildData, _jsonConverter, _publisher);
            Assert.IsNotNull(producer);
        }

        [TestCase(0)]
        [TestCase(11)]
        [TestCase(100)]
        public void TickProducer_ThrowOnCreate_InvalidFrequency(int tickFrequency)
        {
            Assert.Throws<ArgumentException>(() => new TickProducer(tickFrequency, 2, _buildData, _jsonConverter, _publisher));            
        }
        [TestCase(0)]
        [TestCase(101)]
        [TestCase(1001)]
        public void TickProducer_ThrowOnCreate_InvalidTickCount(int tickCount)
        {
            Assert.Throws<ArgumentException>(() => new TickProducer(1, tickCount, _buildData, _jsonConverter, _publisher));
        }

        [Test]
        public void TickProducer_Publish_SingleItem()
        {
            TickProducer producer = new TickProducer(tickFrequency: 1, tickCount: 2, _buildData, _jsonConverter, _publisher);
            Tick tick = _buildData.BuildData("k1", DateTime.UtcNow.ToString(), 0.123456788);
            string refc = _jsonConverter.Serialize(tick);
            producer.Publish(new string[] { refc });
            MockKafkaPublisher.messageQ.TryDequeue(out var message);
            Assert.AreSame(message, refc);
        }

        [Test]
        public void TickProducer_StartStop_ItemsInQueue()
        {
            TickProducer producer = new TickProducer(tickFrequency: 1, tickCount: 2, _buildData, _jsonConverter, _publisher);
            producer.Start();
            Thread.Sleep(2000);
            producer.Stop();
            List<string> items = new List<string>();
            while(MockKafkaPublisher.messageQ.TryDequeue(out var message))
            {
                items.Add(message.ToString()); 
            }
            Assert.IsTrue(items.Any());
        }

        [TearDown] 
        public void TearDown() 
        {
            MockKafkaPublisher.messageQ.Clear();
        }
    }

    public class MockKafkaPublisher : IMessagePublisher
    {
        public static ConcurrentQueue<string> messageQ = new ConcurrentQueue<string>();
        
        public bool IsConnected => true;

        public bool Connect()
        {
            return true;
        }

        public void PublishMessage(string message)
        {
            messageQ.Enqueue(message);
        }

        public void PublishMessages(string[] messages)
        {
            foreach (var message in messages)
                PublishMessage(message);
        }
    }
}
