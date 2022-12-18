using NetModule.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetModule.Module.Internal.Receiver
{
    internal interface IReceiver
    {
        public event Action<BaseMsg> OnReceived;
        public Task<int> GetCount();
        public Task<BaseMsg> Receive();
        public BaseMsg[] ReceiveAll();
    }
}
