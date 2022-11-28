using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This struct is serializable version of System.Int32 .
    /// </summary>
    public struct Int32 : ISerializable
    {
        public Int32(int value)
        {
            Value = value;
        }
        
        /// <summary>
        /// The value.
        /// </summary>
        public int Value { get; set; }
        public ushort InfoLength => sizeof(int);

        public bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            try
            {
                Value = BitConverter.ToInt32(data, startIndex);
                return true;
            }
            catch (DeserializeException e)
            {
                (e.Data[e] as List<string>).Add(GetType().Name);
                throw;
            }
            catch (Exception e)
            {
                DeserializeException exception = new DeserializeException("Deserialization failed.");
                exception.Data.Add(exception, new List<string>() { GetType().Name });
                exception.Data.Add("cause", e);
                throw exception;
            }
        }

        public int Serialize(ref byte[] buffer, int startIndex)
        {
            BitConverter.GetBytes(Value).CopyTo(buffer, startIndex);
            return sizeof(int);
        }

        public static implicit operator int(Int32 msg) => msg.Value;
        public static implicit operator Int32(int value) => new Int32(value);
    }
}
