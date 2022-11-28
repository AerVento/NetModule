using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// A interface for all serializable items.
    /// Every non-abstract class implements this interface should define a paramterless constructor.
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public ushort InfoLength { get; }
        /// <summary>
        /// Serialize this instance and write the byte data to buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write in.</param>
        /// <param name="startIndex">The start index of the buffer to write in. </param>
        /// <returns>The count of bytes have written. </returns>
        public int Serialize(ref byte[] buffer, int startIndex);
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns></returns>
        public bool Deserialize(byte[] data,int startIndex,int endIndex);
    }
}
