using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Module.Internal.Dgram
{
    internal interface IDgram
    {
        /// <summary>
        /// The count of unread Dgram.
        /// </summary>
        public int Count { get; }
        /// <summary>
        /// Read a Dgram.
        /// </summary>
        /// <returns></returns>
        public byte[] Read();
        /// <summary>
        /// Read all Dgram.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> ReadAll();
    }
}
