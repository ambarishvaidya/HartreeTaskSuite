using MessageSink;
using MessageSink.BulkQuery;
using TickData.Model;

namespace TestSuiteApps
{
    public class TestTickSink
    {
        [Test]
        public void BulkQueryPersist_Is_IMessageSink()
        {
            MSSqlBulkQuery<Tick> sql = new MSSqlBulkQuery<Tick>("a", "a");
            Assert.IsTrue(sql is IMessageSink<Tick>);
        }

        [Test]
        public void IMessageSink_Create_IsNotNullForValidInput()
        {
            IMessageSink<Tick> sink = new MSSqlBulkQuery<Tick>("connectionstring", "tablename");
            Assert.IsNotNull(sink);
        }

        [Test]
        public void IMessageSink_Create_QueueIsNotNull()
        {
            IMessageSink<Tick> sink = new MSSqlBulkQuery<Tick>("connectionstring", "tablename");
            Assert.IsNotNull(sink.MessageQueue);
        }

        [TestCase("", "a")]
        [TestCase(null, "a")]
        [TestCase("a", "")]
        [TestCase("a", null)]        
        public void IMessageSink_Throws_ForInvalidInputs(string connectionstring, string tablename)
        {
            Assert.Throws<ArgumentException>(() => new MSSqlBulkQuery<Tick>(connectionstring, tablename));            
        }

        [Test]
        public void IMessageSink_Create_Trigger()
        {
            IMessageSink<Tick> sink = new MSSqlBulkQuery<Tick>("connectionstring", "tablename");
            Assert.IsNotNull(sink.Trigger);
        }
    }
}
