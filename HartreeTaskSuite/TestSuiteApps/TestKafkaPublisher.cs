using MessagePublisher;
using MessagePublisher.ConfluentKafka;

namespace TestSuiteApps
{
    public class TestKafkaPublisher
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void KafkaPublisher_Is_IMessagePublisher()
        {
            KafkaPublisher publisher = new KafkaPublisher("abc", "abc");
            Assert.IsTrue(publisher is IMessagePublisher);
        }

        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("", null)]
        public void KafkaPublisher_Constructor_ThrowsExceptonForIncorrectInpt(string bootstrapServers, string topicName)
        {
            Assert.Throws<ArgumentException>(() => new KafkaPublisher(bootstrapServers, topicName));
        }
    }
}
