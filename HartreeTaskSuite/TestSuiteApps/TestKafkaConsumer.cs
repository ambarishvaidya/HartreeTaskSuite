using MessageConsumer;
using MessageConsumer.ConfluentKafka;
using System.Collections.Concurrent;
using TickData;
using TickData.Interfaces;
using TickData.Model;

namespace TestSuiteApps
{
    public class TestKafkaConsumer
    {
        private IJsonConverter<Tick> _jsonConverter;
        [SetUp]
        public void Setup()
        {
            _jsonConverter = new TickJsonConverter<Tick>();
        }

        [Test]
        public void KafkaConsumer_Is_IMessageConsumer()
        {
            KafkaConsumer<Tick> consumer = new KafkaConsumer<Tick>("a", "b", "c", "d", _jsonConverter);
            Assert.IsTrue(consumer is IMessageConsumer<Tick>);
        }

        [Test]
        public void KafkaConsumer_Create_WithCorrectParameters()
        {
            KafkaConsumer<Tick> consumer = new KafkaConsumer<Tick>("bootstrapserver", "groupid", "offset", "topicname", _jsonConverter);
            Assert.IsNotNull(consumer);
        }

        [TestCase("a","b", "c", null)]
        [TestCase("a", "b", "c", "")]
        [TestCase("a", "b", "", "d")]
        [TestCase("a", "b", null, "d")]
        [TestCase("a", "", "c", "d")]
        [TestCase("a", null, "c", "d")]
        [TestCase("", "b", "c", "d")]
        [TestCase(null, "b", "c", "d")]
        public void KafkaConsumer_ThrowArgumentExceptionOnCreate_WithCorrectParameters(string bootstrapserver, string groupid, string offset, string topicname)
        {
            Assert.Throws<ArgumentException>(() => new KafkaConsumer<Tick>(bootstrapserver, groupid, offset, topicname, _jsonConverter));            
        }

        [Test]
        public void IMessageConsumer_CreateSuccess_HasConcurrentQueue()
        {
            IMessageConsumer<Tick> consumer = new KafkaConsumer<Tick>("bootstrapserver", "groupid", "offset", "topicname", _jsonConverter);
            Assert.That(consumer.MessageQueue is ConcurrentQueue<Tick>);
        }
    }
}
