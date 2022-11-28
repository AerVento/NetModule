using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This struct is serializable version of System.Double .
    /// </summary>
    public struct Double : ISerializable
    {
        public Double(double value)
        {
            Value = value;
        }
        
        /// <summary>
        /// The value.
        /// </summary>
        public double Value { get; set; }
        public ushort InfoLength => sizeof(double);

        public bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            try
            {
                Value = BitConverter.ToDouble(data, startIndex);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public int Serialize(ref byte[] buffer, int startIndex)
        {
            BitConverter.GetBytes(Value).CopyTo(buffer, startIndex);
            return sizeof(double);
        }

        public static implicit operator double(Double msg) => msg.Value;
        public static implicit operator Double(double value) => new Double(value);
    }
}
