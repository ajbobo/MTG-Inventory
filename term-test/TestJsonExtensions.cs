using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ExtensionMethods;

namespace MTG_CLI
{
    [TestClass]
    public class TestJsonExtensions
    {
        private string _json = @"{
                One: 1,
                Two: '2',
                Three: 'three',
                Four: [ 1, 2, 3 ],
                Five: null,
                Six: [ 'a', 'b', 'c' ]
            }";
        private JObject _jObj = new();


        [TestInitialize()]
        public void BeforeAll()
        {
            _jObj = JObject.Parse(_json);
        }

        [TestMethod]
        public void TestAsString()
        {
            string val1 = _jObj["One"].AsString();
            string val2 = _jObj["Two"].AsString();
            string val3 = _jObj["Three"].AsString();
            string val4 = _jObj["Four"].AsString();
            string val5 = _jObj["Five"].AsString();
            string notReal = _jObj["NotReal"].AsString();

            Assert.AreEqual("1", val1);
            Assert.AreEqual("2", val2);
            Assert.AreEqual("three", val3);
            Assert.AreEqual("[\r\n  1,\r\n  2,\r\n  3\r\n]", val4);
            Assert.AreEqual("", val5);
            Assert.AreEqual("", notReal);
        }

        [TestMethod]
        public void TestAsInt()
        {
            int val1 = _jObj["One"].AsInt();
            int val2 = _jObj["Two"].AsInt();
            int val3 = _jObj["Three"].AsInt();
            int val4 = _jObj["Four"].AsInt();
            int val5 = _jObj["Five"].AsInt();
            int notReal = _jObj["NotReal"].AsInt();
            int isNull = ((JToken?)null).AsInt();

            Assert.AreEqual(1, val1);
            Assert.AreEqual(2, val2);
            Assert.AreEqual(0, val3);
            Assert.AreEqual(0, val4);
            Assert.AreEqual(0, val5);
            Assert.AreEqual(0, notReal);
            Assert.AreEqual(0, isNull);
        }

        [TestMethod]
        public void TestHasValue()
        {
            bool val1 = _jObj["One"].HasValue();
            bool val2 = _jObj["Two"].HasValue();
            bool val3 = _jObj["Three"].HasValue();
            bool val4 = _jObj["Four"].HasValue();
            bool val5 = _jObj["Five"].HasValue();
            bool notReal = _jObj["NotReal"].HasValue();

            Assert.AreEqual(true, val1);
            Assert.AreEqual(true, val2);
            Assert.AreEqual(true, val3);
            Assert.AreEqual(true, val4);
            Assert.AreEqual(false, val5);
            Assert.AreEqual(false, notReal);
        }

        [TestMethod]
        public void TestCompressArray()
        {
            string val1 = _jObj["One"].CompressArray();
            string val2 = _jObj["Two"].CompressArray();
            string val3 = _jObj["Three"].CompressArray();
            string val4 = _jObj["Four"].CompressArray();
            string val5 = _jObj["Five"].CompressArray();
            string val6 = _jObj["Six"].CompressArray();
            string notReal = _jObj["NotReal"].CompressArray();

            Assert.AreEqual("", val1);
            Assert.AreEqual("", val2);
            Assert.AreEqual("", val3);
            Assert.AreEqual("123", val4);
            Assert.AreEqual("", val5);
            Assert.AreEqual("abc", val6);
            Assert.AreEqual("", notReal);
        }
    }
}