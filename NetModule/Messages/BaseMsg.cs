using NetModule.Messages.Internal;
using NetModule.Messages.SendingProtocol;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace NetModule.Messages
{
    /// <summary>
    /// A base class for all messages.
    /// A constructor with no parameters are REQUIRED in sub classes. ( No matter it is public or private)
    /// Or a exception will be thrown during the deserialization of the sub class. 
    /// </summary>
    public abstract class BaseMsg : ISerializable
    {
        /// <summary>
        /// The initial bytes count of message id in a message byte array.
        /// Message with generic type will have a longer id.
        /// </summary>
        private const int SIZE_OF_MSGID = sizeof(int);
        /// <summary>
        /// The bytes count of message length in a message byte array.
        /// </summary>
        public const int SIZE_OF_LENGTH = sizeof(int);
        /// <summary>
        /// The offset of total length data in a serialized byte array.
        /// </summary>
        public readonly static int lengthOffset = 2*sizeof(ushort);
        /// <summary>
        /// The protocol to encoding the byte array.
        /// </summary>
        protected static FormProtocol Protocol { get; } = new BasicProtocol();
        
        /// <summary>
        /// The strategy id manager of giving serializable item id.
        /// </summary>
        protected static ISerializableItemIdManager IdManager { get; } = new PlainIdManager();

        /// <summary>
        /// The initial length of sendable message byte array.
        /// Message with generic will have a longer length.
        /// </summary>
        public int Length => Protocol.Length + InfoLength + SIZE_OF_LENGTH + SIZE_OF_MSGID;

        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public abstract ushort InfoLength { get; }
        /// <summary>
        /// Serialize this instance and write the byte data to buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write in.</param>
        /// <param name="startIndex">The start index of the buffer to write in. </param>
        /// <returns>The count of bytes have written. </returns>
        public abstract int Serialize(ref byte[] buffer, int startIndex);
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns>Whether the deserialization succeed.</returns>
        public abstract bool Deserialize(byte[] data, int startIndex, int endIndex);
        
        /// <summary>
        /// To transform this message to a sendable byte array.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"> Thrown when the info length is too small for containing key information.</exception>
        /// <exception cref="NotSupportedException">Thrown when the current type contained a generic type that cannot be serialized.</exception>
        /// <returns>A serialized sendable byte array.</returns>
        public byte[] GetBytes()
        {
            int totalLength = Length;
            
            byte[] msgId;
            Type type = GetType();
            msgId = IdManager.GetGenericId(type);

            totalLength += msgId.Length - SIZE_OF_MSGID;//最开始的id已经计入过length一次

            byte[] bytes = new byte[totalLength];
            int index = 0;

            byte[] identifier = BitConverter.GetBytes(Protocol.Identifier);
            identifier.CopyTo(bytes, index);
            index += identifier.Length;

            byte[] version = BitConverter.GetBytes(Protocol.Version);
            version.CopyTo(bytes, index);
            index += version.Length;

            byte[] length = BitConverter.GetBytes(totalLength);
            length.CopyTo(bytes, index);
            index += length.Length;

            msgId.CopyTo(bytes, index);
            index += msgId.Length;

            int dataLength = Serialize(ref bytes, index);
            index += dataLength;

            byte[] checkCode = BitConverter.GetBytes(Protocol.GetCheckcode(bytes, index));
            checkCode.CopyTo(bytes, index);
            index += checkCode.Length;
            
            return bytes;
        }
        /// <summary>
        /// Get a instance of a type. The type should be a value type or a class type implements ISerializable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The object instance.</returns>
        protected static object GetInstanceOf(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch { }
            return IdManager.GetConstructor(type).Invoke(new object[0]);
        }

        /// <summary>
        /// To transform received byte array to a instance of base message.
        /// </summary>
        /// <param name="data">The given byte array.</param>
        /// <returns>A instance of message.</returns>
        /// <exception cref="MissingMethodException">Thrown when the class of current message id doesn't contain a constructor with no parameters.</exception>
        /// <exception cref="DeserializeException">Thrown when deserialization of key information failed.</exception>
        public static BaseMsg GetInstance(byte[] data)
        {
            return GetInstance(data, 0, data.Length);
        }
        /// <summary>
        /// To transform received byte array to a instance of base message.
        /// If the message of current id doesn't contain a constructor with no parameters, a NullReferenceException will be thrown.
        /// </summary>
        /// <param name="data">The data byte array..</param>
        /// <param name="offset">The offset of byte array.</param>
        /// <param name="length">The length of message.</param>
        /// <returns>A instance of message.</returns>
        /// <exception cref="MissingMethodException">Thrown when the class of current message id doesn't contain a constructor with no parameters.</exception>
        /// <exception cref="DeserializeException">Thrown when deserialization of key information failed.</exception>
        public static BaseMsg GetInstance(byte[] data,int offset,int length)
        {
            offset += 2 * sizeof(ushort);
            offset += SIZE_OF_LENGTH;

            int startIndex = offset;
            int endIndex;
            Type type = IdManager.GetGenericType(data, offset, out endIndex);
            int infoLength = length - Protocol.Length - SIZE_OF_LENGTH - (endIndex - startIndex);

            BaseMsg msg = GetInstanceOf(type) as BaseMsg;

            msg.Deserialize(data, endIndex, endIndex + infoLength);
            return msg;
        }
    }
    /// <summary>
    /// The exception that is thrown during deserializaion.
    /// </summary>
    public class DeserializeException : Exception
    {
        /// <summary>
        /// The byte array that failed for deserialization.
        /// </summary>
        public byte[] failedArray;
        /// <summary>
        /// The real exception occurred during the deserialization.
        /// </summary>
        public Exception realException;
        /// <summary>
        /// The list contains the types during the deserialization steps.
        /// </summary>
        public Stack<Type> deserializationChain = new Stack<Type>();
        /// <summary>
        /// Initialize with a message.
        /// </summary>
        /// <param name="msg">Message</param>
        public DeserializeException(string msg) : base(msg) { }
        /// <summary>
        /// Initialize with a message and real exception.
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="exception">Real exception of deserialize exception.</param>
        public DeserializeException(string msg, Exception exception) : base(msg) { realException = exception; }
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public DeserializeException() { }
    }
}
