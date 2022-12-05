 using NetModule.Messages;
using NetModule.Module.Internal.Dgram;
using NetModule.Module.Internal.Receiver;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetModule.Module
{
    /// <summary>
    /// Udp module.
    /// </summary>
    public class UdpModule
    {
        private ModuleStatus status;

        private ModuleMode mode;

        private UdpClient client;

        private DgramReceiver receiver;

        private IPEndPoint remote;


        /// <summary>
        /// The remote ip end point.
        /// </summary>
        public IPEndPoint Remote => remote;
        /// <summary>
        /// Whether the module can send messages.
        /// </summary>
        public bool CanSend => mode == ModuleMode.Both || mode == ModuleMode.Send;
        /// <summary>
        /// Whether the module can receive messages.
        /// </summary>
        public bool CanReceive => mode == ModuleMode.Both || mode == ModuleMode.Receive;
        
        /// <summary>
        /// The current status of the module.
        /// </summary>
        public ModuleStatus Status => status;
        
        /// <summary>
        /// The count of  unread messages.
        /// </summary>
        public Task<int> Count => receiver.GetCount();

        /// <summary>
        /// To initialize the module with remote end point and a mode.
        /// </summary>
        /// <param name="local"> The local end point.</param>
        /// <param name="remote">The remote end point.</param>
        /// <param name="mode">The mode of the module.</param>
        public UdpModule(IPEndPoint local, IPEndPoint remote, ModuleMode mode)
        {
            this.remote = remote;
            this.mode = mode;
            client = new UdpClient(local);
            receiver = new DgramReceiver(new SocketDgram(local, remote));
            status = ModuleStatus.Initialized;
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <exception cref="InvalidCastException">Thrown when module is not in send mode.</exception>
        public void Send(BaseMsg msg)
        {
            if (CanSend)
            {
                Task.Run(() =>
                {
                    byte[] arr = msg.GetBytes();
                    client.Send(arr, arr.Length, remote);
                });
            }
            else
            {
                throw new InvalidOperationException("The module is not in send mode.");
            }
        }

        /// <summary>
        /// Receive a unread message.
        /// </summary>
        /// <returns>A unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public Task<BaseMsg> Receive()
        {
            if (CanReceive)
            {
                return receiver.Receive();
            }
            else
                throw new InvalidOperationException("The module is not in receive mode.");
        }
        /// <summary>
        /// Return all unread messages.
        /// </summary>
        /// <returns>All unread messages, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public BaseMsg[] ReceiveAll()
        {
            if (CanReceive)
            {
                return receiver.ReceiveAll();
            }
            else
                throw new InvalidOperationException("The module is not in receive mode.");
        }
    }
}
