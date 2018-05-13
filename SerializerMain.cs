using System;
using System.Text;

namespace GenericSerializer
{
    internal class SerializerMain
    {
        private static void Main()
        {
            var serial = new Serializer();
            serial.AddInt32(0);
            serial.AddString("Pb4");
            serial.AddString("Pb4.1");
            serial.AddString("Update");

            var final = serial.FinalizeSerializationProcess();

            var des = new Deserializer(final);

            var i1 = des.GetInt32();
            var s1 = des.GetString();
            var s2 = des.GetString();
            var s3 = des.GetString();
            Console.WriteLine("blub");
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }

    public class Deserializer
    {
        private readonly string _serializedData;
        private int _currentPosition;

        public Deserializer(string serializedData)
        {
            _serializedData = serializedData;
            CurrentPosition = 0;
        }

        public int CurrentPosition
        {
            get { return _currentPosition; }
            private set
            {
                if (value > _serializedData.Length)
                    throw new ParserException("CurrentPosition out of bounds of char array");

                _currentPosition = value;
            }
        }

        public long GetLong()
        {
            var dataSize = 19;
            //setting forward the currentPos pointer,
            //to make sure we get an early exception
            //if we move out of bounds

            var multiplicator = 1;
            if (_serializedData.ToCharArray()[CurrentPosition] == '-')
            {
                multiplicator = -1;
                CurrentPosition++;
            }

            CurrentPosition += dataSize;
            long ret;
            if (!long.TryParse(_serializedData.Substring(CurrentPosition - dataSize, dataSize), out ret))
                throw new ParserException($"Error parsing variable of type long at index {CurrentPosition - dataSize}");
            return ret * multiplicator;
        }

        public int GetInt32()
        {
            CurrentPositionCheck();
            var dataSize = 10;
            //setting forward the currentPos pointer,
            //to make sure we get an early exception
            //if we move out of bounds
            var multiplicator = 1;

            if (_serializedData.ToCharArray()[CurrentPosition] == '-')
            {
                multiplicator = -1;
                CurrentPosition++;
            }

            CurrentPosition += dataSize;

            int ret;
            if (!int.TryParse(_serializedData.Substring(CurrentPosition - dataSize, dataSize), out ret))
                throw new ParserException($"Error parsing variable of type int at index {CurrentPosition - dataSize}");
            return ret * multiplicator;
        }

        public string GetString()
        {
            var stringLenght = GetInt32();
            if (stringLenght == 0)
                return string.Empty;

            CurrentPositionCheck();


            var ret = new StringBuilder();
            for (var i = 0; i < stringLenght; i++)
            {
                ret.Append(_serializedData.ToCharArray()[CurrentPosition]);
                CurrentPosition++;
            }

            return ret.ToString();
        }

        public bool GetBool()
        {
            CurrentPositionCheck();
            CurrentPosition++;
            return _serializedData.ToCharArray()[CurrentPosition - 1] == '1';
        }

        public double GetDouble()
        {
            CurrentPositionCheck();
            var arrayData = new byte[8];
            //serialized doubles are always 8 byte arrays
            for (var i = 0; i < 8; i++)
            {
                //every byte in the string is coded as number, that is padded with zeros to 3 places
                arrayData[i] = byte.Parse(_serializedData.Substring(CurrentPosition, 3));
                CurrentPosition += 3;
            }

            return BitConverter.ToDouble(arrayData, 0);
        }

        private void CurrentPositionCheck()
        {
            if (CurrentPosition >= _serializedData.Length)
                throw new ParserException("Index out of bounds on parsing Value");
        }
    }

    public class Serializer
    {
        private readonly StringBuilder _serializer;
        private bool _finalized;

        public Serializer()
        {
            _serializer = new StringBuilder();
        }

        public void AddString(string data)
        {
            FinalizedException();
            //pad the lenght with 0s
            AddInt32(data.Length);
            _serializer.Append(data);
        }

        public void AddInt32(int data)
        {
            
            FinalizedException();
            _serializer.Append(data.ToString("D10"));
        }

        public void AddLong(long data)
        {
            FinalizedException();
            _serializer.Append(data.ToString("D19"));
        }

        public void AddBool(bool data)
        {
            _serializer.Append(data ? "1" : "0");
        }

        public void AddDouble(double data)
        {
            var dataBytes = BitConverter.GetBytes(data);
            foreach (var t in dataBytes) _serializer.Append(t.ToString("D3"));
        }

        private void FinalizedException()
        {
            if (_finalized) throw new ParserException("Tried to append to finalized Serializer");
        }

        public string FinalizeSerializationProcess()
        {
            _finalized = true;
            return _serializer.ToString();
        }
    }

    public interface IMySerializable
    {
        void Serialize(Serializer appendTo);
        void Deserialze(Deserializer serializedData);
    }

    //example class for serializer
    class TestSerialzer : IMySerializable
    {
        public int Field1;
        public int Field2;
        public string Field3;
        public bool Field4;
        public NestedTestSerialzer nts;

        public TestSerialzer(int f1, int f2, string f3, bool f4, int nf1, string nf2)
        {
            Field1 = f1;
            Field2 = f2;
            Field3 = f3;
            Field4 = f4;
            nts = new NestedTestSerialzer(nf1, nf2);
        }

        //empty constructor needs to be existant, so you can use the deserialize method on the object
        public TestSerialzer() { }

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

    class NestedTestSerialzer : IMySerializable
    {
        public int Field1;
        public string Field2;

        public NestedTestSerialzer(int field1, string field2)
        {
            Field1 = field1;
            Field2 = field2;
        }

        public NestedTestSerialzer() {}

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