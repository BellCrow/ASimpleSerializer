using System;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace GenericSerializer
{
    [TestFixture]
    internal class SerializerTest
    {
        [SetUp]
        public void SetUp()
        {
            ser = new Serializer();
        }

        private Serializer ser;

        [Test]
        public void Append2BoolsLegalValuesSuccess()
        {
            ser.AddBool(true);
            ser.AddBool(false);
            var final = ser.FinalizeSerializationProcess();

            Assert.AreEqual(final, "10");
        }

        [Test]
        public void Append2DoublesLegalValuesSuccess()
        {
            string[] excpected1 = {"000", "061", "145", "096", "228", "088", "225", "067"};
            string[] excpected2 = {"144", "113", "188", "147", "132", "067", "201", "194"};

            ser.AddDouble(9999999999999999999.9999999999999999999999999999);
            ser.AddDouble(-55555555555555.123465789);

            var final = ser.FinalizeSerializationProcess();
            var stringIterator = 0;
            //check first number
            foreach (var excpectedVal in excpected1)
            {
                Assert.AreEqual(excpectedVal, final.Substring(stringIterator, 3));
                stringIterator += 3;
            }

            //check second number
            foreach (var excpectedVal in excpected2)
            {
                Assert.AreEqual(excpectedVal, final.Substring(stringIterator, 3));
                stringIterator += 3;
            }
        }

        [Test]
        public void Append2StringsAnd2NumbersLegalValuesSucces()
        {
            ser.AddString("test123");
            ser.AddInt32(012345);
            ser.AddString("helloThisIsATest");
            ser.AddLong(34589);
            var final = ser.FinalizeSerializationProcess();

            Assert.AreEqual(final, "0000000007test123" +
                                   "0000012345" +
                                   "0000000016helloThisIsATest" +
                                   "0000000000000034589");
        }

        [Test]
        public void Append2StringsLegalStringsSucces()
        {
            ser.AddString("test123");
            ser.AddString("helloThisIsATest");
            var final = ser.FinalizeSerializationProcess();

            Assert.AreEqual(final, "0000000007test123" +
                                   "0000000016helloThisIsATest");
        }

        [Test]
        public void Append4IntsLegalIntSucces()
        {
            ser.AddInt32(123);
            ser.AddInt32(5);
            ser.AddInt32(8);
            ser.AddInt32(99);

            var final = ser.FinalizeSerializationProcess();

            Assert.AreEqual(final, "0000000123" +
                                   "0000000005" +
                                   "0000000008" +
                                   "0000000099");
        }

        [Test]
        public void AppendIntLegalIntSucces()
        {
            ser.AddInt32(123);

            var final = ser.FinalizeSerializationProcess();

            Assert.AreEqual(final, "0000000123");
        }

        [Test]
        public void CantUseFinalizedSerialzerFailsWithException()
        {
            var ser = new Serializer();
            ser.AddString("Just some TestData");
            ser.FinalizeSerializationProcess();

            Assert.Throws<ParserException>(() => ser.AddString("This call should fail"));
        }
    }

    [TestFixture]
    internal class DeserialzerTest
    {
        [Test]
        public void Get2DoublesFromLegalDataSuccess()
        {
            var data = "000061145096228088225067144113188147132067201194";

            var des = new Deserializer(data);
            Assert.AreEqual(9999999999999999999.9999999999999999999999999999, des.GetDouble());
            //Assert.AreEqual(-55555555555555.123465789, des.GetDouble());
        }

        [Test]
        public void GetStringFromDataWithOneStringAndOneIntSucces()
        {
            var originalData = "testHalloHelp";
            var data = originalData.Length.ToString("D10") + originalData;
            data = 123.ToString("D10") + data + 456.ToString("D10");
            var des = new Deserializer(data);
            Assert.AreEqual(123, des.GetInt32());
            Assert.IsTrue(des.GetString() == originalData);
            Assert.AreEqual(456, des.GetInt32());
        }

        [Test]
        public void GetStringFromDataWithOneStringSucces()
        {
            var originalData = "testHalloHelp";
            var data = originalData.Length.ToString("D10") + originalData;
            var des = new Deserializer(data);
            var result = des.GetString();
            Assert.IsTrue(result == originalData);
        }

        [Test]
        public void Read1NegatveIntAndThenTryToReadAnInvalidIntThrowsException()
        {
            var data = "-0000000005" +
                       "-000000001"; //here is a 0 missing on purpose
            var des = new Deserializer(data);

            Assert.AreEqual(-5, des.GetInt32());
            Assert.Throws<ParserException>(() => { des.GetInt32(); });
        }

        [Test]
        public void Read2NegativeAnd1PositiveIntFromDataSuccess()
        {
            var data = "-0000000005" +
                       "-0000000001" +
                       "0000000000";
            var des = new Deserializer(data);

            Assert.AreEqual(-5, des.GetInt32());
            Assert.AreEqual(-1, des.GetInt32());
            Assert.AreEqual(0, des.GetInt32());
        }

        [Test]
        public void ReadIntFromArrayWithLegalDataFor1IntSuccess()
        {
            var data = "0000001234";
            var des = new Deserializer(data);
            Assert.AreEqual(1234, des.GetInt32());
            Assert.AreEqual(10, des.CurrentPosition);
        }

        [Test]
        public void ReadIntFromArrayWithLegalDataFor1NegativeIntSuccess()
        {
            var data = "-0000001234";
            var des = new Deserializer(data);
            Assert.AreEqual(-1234, des.GetInt32());
            Assert.AreEqual(11, des.CurrentPosition);
        }

        [Test]
        public void ReadIntFromArrayWithLegalDataFor4IntsButTryToRead5FailsWithException()
        {
            var data = "0000001234" +
                       "0000002345" +
                       "0000003456" +
                       "0000004567";
            var des = new Deserializer(data);

            Assert.AreEqual(1234, des.GetInt32());
            Assert.AreEqual(2345, des.GetInt32());
            Assert.AreEqual(3456, des.GetInt32());
            Assert.AreEqual(4567, des.GetInt32());
            Assert.Throws<ParserException>(
                () => { des.GetInt32(); });
        }

        [Test]
        public void ReadIntFromArrayWithLegalDataFor4IntsSuccess()
        {
            var data = "0000001234" +
                       "0000002345" +
                       "0000003456" +
                       "0000004567";
            var des = new Deserializer(data);

            Assert.AreEqual(1234, des.GetInt32());
            Assert.AreEqual(2345, des.GetInt32());
            Assert.AreEqual(3456, des.GetInt32());
            Assert.AreEqual(4567, des.GetInt32());
            Assert.AreEqual(40, des.CurrentPosition);
        }

        [Test]
        public void ReadStringAndBoolFromStringSuccess()
        {
            var data = "0000000005test1" +
                       "1" +
                       "0";
            var des = new Deserializer(data);

            Assert.AreEqual("test1", des.GetString());
            Assert.AreEqual(true, des.GetBool());
            Assert.AreEqual(false, des.GetBool());
        }
    }

    [TestFixture]
    internal class IntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            ser = new Serializer();
        }

        private Serializer ser;

        private long LongRandom(long min, long max, Random rand)
        {
            var buf = new byte[8];
            rand.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }

        [Test]
        public void Serialize2IntsAndDeserialzeStringThrowsException()
        {
            var originalvalue = -12569;
            var originalvalue2 = 987654;

            ser.AddInt32(originalvalue);
            ser.AddInt32(originalvalue2);
            var serializedData = ser.FinalizeSerializationProcess();
            var des = new Deserializer(serializedData);
            Assert.AreEqual(originalvalue, des.GetInt32());
            Assert.Throws<ParserException>(() => { des.GetString(); });
        }

        [Test]
        public void SerializeAndDeserialize4RandomValuesOfDifferentType()
        {
            var rand = new Random(Guid.NewGuid().GetHashCode());
            var value1 = LongRandom(0, long.MaxValue, rand);
            var value2 = rand.Next(0, int.MaxValue);
            double value3 = LongRandom(0, long.MinValue, rand) + rand.Next(0, 5000) / 1000;
            var value4 = rand.Next() % 2 == 0;
            var value5Builder = new StringBuilder();
            for (var i = 0; i < 50; i++)
            {
                //just using numbers as string. should still work
                value5Builder.Append(rand.Next());
            }

            var value5 = value5Builder.ToString();
            TestContext.Out.WriteLine($"long value:{value1}");
            TestContext.Out.WriteLine($"int value:{value2}");
            TestContext.Out.WriteLine($"double value:{value3}");
            TestContext.Out.WriteLine($"bool value:{value4}");
            TestContext.Out.WriteLine($"string value:{value5}");


            ser.AddLong(value1);
            ser.AddInt32(value2);
            ser.AddDouble(value3);
            ser.AddBool(value4);
            ser.AddString(value5);

            var final = ser.FinalizeSerializationProcess();
            var des = new Deserializer(final);
            Assert.AreEqual(value1, des.GetLong());
            Assert.AreEqual(value2, des.GetInt32());
            Assert.AreEqual(value3, des.GetDouble());
            Assert.AreEqual(value4, des.GetBool());
            var value = des.GetString();
            Assert.AreEqual(value5, value);
        }

        [Test]
        public void SerialzeAndDeserialze1StringSuccess()
        {
            var originalvalue = "MyTestString";

            ser.AddString(originalvalue);
            var serializedData = ser.FinalizeSerializationProcess();
            var des = new Deserializer(serializedData);
            Assert.AreEqual(originalvalue, des.GetString());
        }

        [Test]
        public void SerialzeAndDeserialze2IntSuccess()
        {
            var originalvalue = 246799999;
            var originalvalue2 = -598746565;


            ser.AddInt32(originalvalue);
            ser.AddInt32(originalvalue2);
            var serializedData = ser.FinalizeSerializationProcess();
            var des = new Deserializer(serializedData);
            Assert.AreEqual(originalvalue, des.GetInt32());
            Assert.AreEqual(originalvalue2, des.GetInt32());
        }

        [Test]
        public void SerialzeAndDeserialze2LongSuccess()
        {
            var originalvalue = -2344556677888888877;
            var originalvalue2 = 2344556677888888877;


            ser.AddLong(originalvalue);
            ser.AddLong(originalvalue2);
            var serializedData = ser.FinalizeSerializationProcess();
            var des = new Deserializer(serializedData);
            Assert.AreEqual(originalvalue, des.GetLong());
            Assert.AreEqual(originalvalue2, des.GetLong());
        }

        [Test]
        public void SerializeObjectAndDeserializeItAgain()
        {
            int f1 = 20;
            int f2 = -40;
            string f3 = "This is a testString";
            bool f4 = false;

            int nf1 = -int.MaxValue;
            string nf2 = string.Empty;

            var t = new TestSerialzer(f1,f2,f3,f4,nf1,nf2);
            t.Serialize(ser);

            string serialize = ser.FinalizeSerializationProcess();

            var restored = new TestSerialzer();
            restored.Deserialze(new Deserializer(serialize));

            Assert.AreEqual(f1,restored.Field1);
            Assert.AreEqual(f2,restored.Field2);
            Assert.AreEqual(f3,restored.Field3);
            Assert.AreEqual(f4,restored.Field4);

            Assert.AreEqual(nf1, restored.nts.Field1);
            Assert.AreEqual(nf2, restored.nts.Field2);
        }

        //example class for serializer
        private class TestSerialzer : IMySerializable
        {
            public int Field1;
            public int Field2;
            public string Field3;
            public bool Field4;
            public NestedTestSerialzer nts;

            public TestSerialzer(int f1, int f2, string f3,bool f4,int nf1,string nf2)
            {
                Field1 = f1;
                Field2 = f2;
                Field3 = f3;
                Field4 = f4;
                nts = new NestedTestSerialzer(nf1,nf2);
            }

            //empty constructor needs to be existant, so you can use the deserialize method on the object
            public TestSerialzer(){}

            public void Serialize(Serializer appendTo)
            {
                appendTo.AddInt32(Field1);
                appendTo.AddInt32(Field2);
                appendTo.AddString(Field3);
                nts.Serialize(appendTo);
            }

            public void Deserialze(Deserializer serializedData)
            {
                Field1 = serializedData.GetInt32();
                Field2 = serializedData.GetInt32();
                Field3 = serializedData.GetString();
                nts = new NestedTestSerialzer();
                nts.Deserialze(serializedData);
            }
        }

        private class NestedTestSerialzer : IMySerializable
        {
            public int Field1;
            public string Field2;

            public NestedTestSerialzer(int field1, string field2)
            {
                Field1 = field1;
                Field2 = field2;
            }

            public NestedTestSerialzer(){}

            public void Serialize(Serializer appendTo)
            {
                appendTo.AddInt32(Field1);
                appendTo.AddString(Field2);
            }

            public void Deserialze(Deserializer serializedData)
            {
                Field1 = serializedData.GetInt32();
                Field2 = serializedData.GetString();

            }
        }
    }
}