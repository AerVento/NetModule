using NetModule.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetModule.Module
{
    /// <summary>
    /// Not available.
    /// </summary>
    class UdpModule : INetModule
    {
        private ModuleStatus status;

        private ModuleMode mode;

        private Socket socket;
        
        private IPEndPoint remote;
        
        public IPEndPoint Remote => remote;

        public bool CanSend => mode == ModuleMode.Both || mode == ModuleMode.Send;
        
        public bool CanReceive => mode == ModuleMode.Both || mode == ModuleMode.Receive;

        public ModuleStatus Status => status;

        public int Available => throw new NotImplementedException();

        public bool IsTimeOut => throw new NotImplementedException();

        public event Action<Exception> onError;

        public UdpModule(IPEndPoint remote, ModuleMode mode)
        {
            this.remote = remote;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(remote);
            status = ModuleStatus.Initialized;
        }

        public void Send(BaseMsg msg)
        {
            throw new NotImplementedException();
        }

        public BaseMsg Receive()
        {
            throw new NotImplementedException();
        }

        public BaseMsg[] ReceiveAll()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            status = ModuleStatus.Connecting;
        }

        public void Close()
        {
            status = ModuleStatus.Closed;
        }
    }
}
