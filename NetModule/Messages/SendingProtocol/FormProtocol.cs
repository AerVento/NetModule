using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages.SendingProtocol
{
    /// <summary>
    /// Use extra data to define a protocol when passing information.
    /// </summary>
    public abstract class FormProtocol
    {
        /// <summary>
        /// The msg identifier in the byte array.
        /// </summary>
        public abstract ushort Identifier { get; }
        /// <summary>
        /// The version of the protocol.
        /// </summary>
        public abstract ushort Version { get; }
        /// <summary>
        /// Calculate the check code of byte array.
        /// </summary>
        /// <param name="data"> All bytes used to calculate the check code.</param>
        /// <param name="count"> The count of bytes.</param>
        /// <returns>The code</returns>
        public abstract ushort GetCheckcode(byte[] data, int count);
        /// <summary>
        /// Return the length of the protocol elements.
        /// </summary>
        public abstract int Length { get; }
    }
}
