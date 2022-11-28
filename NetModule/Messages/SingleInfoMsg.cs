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
        protected T content;
        /// <summary>
        /// The information of message.
        /// </summary>
        public T Content => content;

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

        public override ushort InfoLength => (ushort)(content.InfoLength);

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

        public override int Serialize(ref byte[] buffer,int startIndex)
        {
            return content.Serialize(ref buffer,startIndex);
        }

    }
}
