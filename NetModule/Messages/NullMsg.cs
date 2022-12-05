using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// A message to mark the useless message.
    /// Only can be discovered in Receive() methods when receiving a message.
    /// </summary>
    public class NullMsg:EmptyMsg
    {
        /// <summary>
        /// static operator
        /// </summary>
        /// <param name="msg"></param>
        public static implicit operator BaseMsg[](NullMsg msg) => new BaseMsg[1] {msg};
    }
}
