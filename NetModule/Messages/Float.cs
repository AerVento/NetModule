using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This struct is serializable version of System.Single .
    /// </summary>
    public struct Float : ISerializable
    {
        public Float(float value)
        {
            Value = value;
        }

        /// <summary>
        /// The value.
        /// </summary>
        public float Value { get; set; }
        public ushort InfoLength => sizeof(float);

        public bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            try
            {
                Value = BitConverter.ToSingle(data, startIndex);
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
            return sizeof(float);
        }

        public static implicit operator float(Float msg) => msg.Value;
        public static implicit operator Float(float value) => new Float(value);
    }
}
