 using NetModule.Messages;
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
    public class UdpModule : INetModule
    {
        private ModuleStatus status;

        private ModuleMode mode;

        private UdpClient client;
        
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
        /// The count of bytes of unread messages.
        /// </summary>
        public int Available => client.Available;
        /// <summary>
        /// Whether the remote haven't sent message for a time.
        /// </summary>
        public bool IsTimeOut => false;//Udp connection do not need to be time out.
        /// <summary>
        /// A action called when error occurred.
        /// </summary>
        public event Action<Exception> onError;
        /// <summary>
        /// To initialize the module with remote end point and a mode.
        /// </summary>
        /// <param name="local"> The local end point.</param>
        /// <param name="remote">The remote end point.</param>
        /// <param name="mode">The mode of the module.</param>
        public UdpModule(IPEndPoint local,IPEndPoint remote, ModuleMode mode)
        {
            this.remote = remote;
            client = new UdpClient(local);
            status = ModuleStatus.Initialized;
            this.mode = mode;
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
        public BaseMsg Receive()
        {
            if (CanReceive)
            {
                if (client.Available > 0)
                    return BaseMsg.GetInstance(client.Receive(ref remote));
                else
                    return null;
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
                if (client.Available > 0)
                {
                    int count = client.Available;
                    BaseMsg[] arr = new BaseMsg[count];
                    for (int i = 0; i < count; i++)
                    {
                        arr[i] = BaseMsg.GetInstance(client.Receive(ref remote));
                    }
                    return arr;
                }
                else
                    return null;
            }
            else
                throw new InvalidOperationException("The module is not in receive mode.");
        }
        /// <summary>
        /// To start the module.
        /// </summary>
        public void Start()
        {
            status = ModuleStatus.Connecting;
        }
        /// <summary>
        /// To close the module.
        /// </summary>
        public void Close()
        {
            status = ModuleStatus.Closed;
        }
    }
}
