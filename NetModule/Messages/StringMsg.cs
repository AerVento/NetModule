using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This message class contained a string to present extra information.
    /// </summary>
    public class StringMsg : BaseMsg
    {
        /// <summary>
        /// The byte data of string.
        /// </summary>
        protected byte[] datas;
        /// <summary>
        /// The encoding of the string.
        /// </summary>
        protected Encoding encoding;
        /// <summary>
        /// The string information.
        /// </summary>
        public string String => encoding.GetString(datas);
        /// <summary>
        /// For activator use only.
        /// </summary>
        protected StringMsg()
        {

        }
        /// <summary>
        /// Use default encoding to encode the string.
        /// </summary>
        /// <param name="str">The string information.</param>
        public StringMsg(string str)
        {
            this.encoding = Encoding.Default;
            this.datas = encoding.GetBytes(str);
        }
        /// <summary>
        /// Use specific encoding to encode the string.
        /// </summary>
        /// <param name="str">The string information.</param>
        /// <param name="encoding">The encoding of the string.</param>
        public StringMsg(string str,Encoding encoding)
        {
            this.encoding = encoding;
            this.datas = encoding.GetBytes(str);
        }
        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public override ushort InfoLength => (ushort)(sizeof(int) + datas.Length);
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns>Whether the deserialization succeed.</returns>
        public override bool Deserialize(byte[] data,int startIndex, int endIndex)
        {
            try
            {
                encoding = Encoding.GetEncoding(BitConverter.ToInt32(data,startIndex));
                datas = new byte[endIndex - startIndex - sizeof(int)];
                Array.Copy(data, startIndex + sizeof(int), datas, 0, endIndex - startIndex - sizeof(int));
                return true;
            }
            catch(DeserializeException e)
            {
                e.deserializationChain.Push(GetType());
                throw;
            }
            catch(Exception e)
            {
                DeserializeException exception = new DeserializeException("Deserialization failed.",e);
                exception.failedArray = new byte[endIndex - startIndex];
                Array.Copy(data, startIndex, exception.failedArray, 0, endIndex - startIndex);
                exception.deserializationChain.Push(GetType());
                throw exception;
            }
        }
        /// <summary>
        /// Serialize this instance and write the byte data to buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write in.</param>
        /// <param name="startIndex">The start index of the buffer to write in. </param>
        /// <returns>The count of bytes have written. </returns>
        public override int Serialize(ref byte[] buffer, int startIndex)
        {
            BitConverter.GetBytes(encoding.CodePage).CopyTo(buffer, startIndex);
            Array.Copy(datas, 0, buffer, startIndex + sizeof(int), datas.Length);
            return sizeof(int) + datas.Length;
        }


        public static implicit operator string(StringMsg msg) => msg.String;
        public static implicit operator StringMsg(string str) => new StringMsg(str);
    }
}
