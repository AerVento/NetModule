using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NetModule.Module.Internal.Stream
{
    internal interface IStream
    {
        /// <summary>
        /// The count of bytes in stream.
        /// </summary>
        public int Available { get; }
        /// <summary>
        /// Read a byte from stream.
        /// </summary>
        /// <returns>The byte of stream, or -1 when end of stream is reached.</returns>
        public int Read();
        /// <summary>
        /// Read all bytes from stream.
        /// </summary>
        /// <returns>The byte array of all bytes in the stream.</returns>
        public byte[] ReadAll();
    }
}
