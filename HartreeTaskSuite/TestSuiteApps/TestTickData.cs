using TickData;
using TickData.Interfaces;
using TickData.Model;

namespace TestSuiteApps
{
    public class TestTickData
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TickBuilder_Implements_IBuildData()
        {
            TickBuilder builder = new TickBuilder();
            Assert.IsTrue(builder is IBuildData);
        }

        [Test]
        public void BuildData_ReturnsTick_ForCorrectData()
        {
            IBuildData builder = new TickBuilder();
            var data = builder.BuildData("key", "time", 0.123456);
            Assert.IsNotNull(data);
        }

        [TestCase("", "time", 0.12345)]
        [TestCase("key", "", 0.12345)]
        [TestCase(null, "time", 0.12345)]
        [TestCase("key", null, 0.12345)]
        public void BuildData_ReturnsNull_ForInCorrectData(string key, string utcTime, double value)
        {
            IBuildData builder = new TickBuilder();
            var data = builder.BuildData(key, utcTime, value);
            Assert.IsNull(data);
        }

        [Test]
        public void Tick_ValidateData_ForCorrectInput()
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time", 0.123456);
            var data2 = builder.BuildData("key", "time", 0.123456);
            Assert.IsTrue(data1 == data2);
        }

        [Test]
        public void Tick_ValidateData_ForInCorrectInput()
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time1", 0.123456);
            var data2 = builder.BuildData("key", "time", 0.123456);
            Assert.IsFalse(data1 == data2);
        }

        [TestCase(0.1234, 0.12345)]
        [TestCase(0.999999, 0.999)]
        public void Tick_ValidateIncorrectData_ForInCorrectDblInput(double d1, double d2)
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time", d1);
            var data2 = builder.BuildData("key", "time", d2);
            Assert.IsTrue(data1 != data2);
        }

        [Test]
        public void Tick_CheckEquals_ForCorrectInput()
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time", 0.123456);
            var data2 = builder.BuildData("key", "time", 0.123456);
            Assert.IsTrue(data1.Equals(data2));
        }

        [Test]
        public void Tick_CheckEquals_ForInCorrectInput()
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time", 0.123456);
            var data2 = builder.BuildData("key", "time", 0.123556);
            Assert.IsFalse(data1.Equals(data2));
        }

        [Test]
        public void Tick_CheckEquals_ForNullInput()
        {
            IBuildData builder = new TickBuilder();
            var data1 = builder.BuildData("key", "time", 0.123456);
            Assert.IsFalse(data1.Equals(null));
        }

        [Test]
        public void JsonSerializer_Is_IJsonConverter()
        {
            TickJsonConverter<Tick> ser = new TickJsonConverter<Tick>();
            Assert.IsTrue(ser is IJsonConverter<Tick>);
        }

        [Test]
        public void TickJsonConverter_Serialize_ForCorrectInput()
        {
            string requiredFormat = "{\"key\":\"k1\",\"value\":{\"time\":\"t1\",\"value\":0.15}}";
            IBuildData builder = new TickBuilder();
            Tick data = builder.BuildData("k1", "t1", 0.15);
            IJsonConverter<Tick> json = new TickJsonConverter<Tick>();
            string resp = json.Serialize(data);
            Assert.AreEqual(requiredFormat, resp);
        }

        [Test]
        public void TickJsonConverter_DeSerialize_ForCorrectInput()
        {
            string input = "{\"key\":\"k1\",\"value\":{\"time\":\"t1\",\"value\":0.15}}";
            IBuildData builder = new TickBuilder();
            Tick refData = builder.BuildData("k1", "t1", 0.15);
            IJsonConverter<Tick> json = new TickJsonConverter<Tick>();
            Tick resp = json.Deserialize(input);
            Assert.AreEqual(refData, resp);
        }
    }
}