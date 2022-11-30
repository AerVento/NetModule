using NetModule.Messages.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetModule.Messages
{
    /// <summary>
    /// This class contains a generic type of extra information.
    /// </summary>
    /// <typeparam name="T">The type of single information.</typeparam>
    public class SingleInfoMsg<T> : BaseMsg where T : ISerializable
    {
        /// <summary>
        /// The private content data.
        /// </summary>
        protected T content;
        /// <summary>
        /// The information of message.
        /// </summary>
        public T Content => content;
        /// <summary>
        /// For activator use only.
        /// </summary>
        protected SingleInfoMsg()
        {

        }
        /// <summary>
        /// Use the info to initialize this message class.
        /// </summary>
        /// <param name="info">The extra information of type T.</param>
        public SingleInfoMsg(T info)
        {
            this.content = info;
        }
        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public override ushort InfoLength => (ushort)(content.InfoLength);
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns>Whether the deserialization succeed.</returns>
        public override bool Deserialize(byte[] data,int startIndex,int endIndex)
        {
            try
            {
                content = (T)GetInstanceOf(typeof(T));
                return content.Deserialize(data, startIndex, endIndex);
            }
            catch (DeserializeException e)
            {
                e.deserializationChain.Push(GetType());
                throw;
            }
            catch (Exception e)
            {
                DeserializeException exception = new DeserializeException("Deserialization failed.", e);
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
        public override int Serialize(ref byte[] buffer,int startIndex)
        {
            return content.Serialize(ref buffer,startIndex);
        }

    }
}
