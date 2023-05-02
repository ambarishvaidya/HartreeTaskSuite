using Confluent.Kafka;
using Consumer;
using MessageConsumer;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TickData;
using TickData.Interfaces;
using TickData.Model;

namespace TestSuiteApps
{
    public class TestTickConsumer
    {
        private IMessageConsumer<Tick> _tickConsumer;

        [SetUp]
        public void Setup()
        {
            _tickConsumer = new MockTickConsumer<Tick>();
        }

        [Test]
        public void TickConsumer_Is_IConsumer()
        {
            TickConsumer<Tick> consumer = new TickConsumer<Tick>(1, _tickConsumer);
            Assert.IsTrue(consumer is IConsumer<Tick>);
        }

        [Test]
        public void ICunsumer_IsNotNull_ForValidInput()
        {
            IConsumer<Tick> consumer = new TickConsumer<Tick>(1, _tickConsumer);
            Assert.IsNotNull(consumer);
        }
    }

    public class MockTickConsumer<T> : IMessageConsumer<T> where T : class
    {
        public ConcurrentQueue<T> MessageQueue => throw new NotImplementedException();

        public bool IsSubscribed => throw new NotImplementedException();

        public void StartSubscription(CancellationTokenSource cts)
        {
            throw new NotImplementedException();
        }

        public void StopSubscription()
        {
            throw new NotImplementedException();
        }
    }
}
