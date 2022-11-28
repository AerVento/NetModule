using NetModule.Messages;
using System;
using System.Net;
using System.Net.Sockets;

namespace NetModule.Module
{
    /// <summary>
    /// The module mode of sending or receiving messages.
    /// </summary>
    public enum ModuleMode
    {
        /// <summary>
        /// The module can send the message to remote.
        /// </summary>
        Send,
        /// <summary>
        /// The module can receive the message from remote.
        /// </summary>
        Receive,
        /// <summary>
        /// The module can both send and receive messages.
        /// </summary>
        Both
    }
    /// <summary>
    /// The current status of the module.
    /// </summary>
    public enum ModuleStatus
    {
        /// <summary>
        /// The module initialized internal data and wait for being started for first time.
        /// </summary>
        Initialized,
        /// <summary>
        /// The module has connected to remote.
        /// </summary>
        Connecting,
        /// <summary>
        /// The module is closed and wait for being start again.
        /// </summary>
        Closed,
    }

    /// <summary>
    /// A interface of net module. Every net module contains these methods.
    /// </summary>
    public interface INetModule
    {
        /// <summary>
        /// A action called when error happened.
        /// </summary>
        public event Action<Exception> onError;

        /// <summary>
        /// The remote ip end point.
        /// </summary>
        public IPEndPoint Remote { get; }
        /// <summary>
        /// Whether the module can send messages.
        /// </summary>
        public bool CanSend { get; }
        /// <summary>
        /// Whether the module can receive messages.
        /// </summary>
        public bool CanReceive { get; }
        /// <summary>
        /// The current status of the module.
        /// </summary>
        public ModuleStatus Status { get; }
        /// <summary>
        /// The count of unread messages.
        /// </summary>
        public int Available { get; }
        /// <summary>
        /// Whether the remote haven't send message for a time. 
        /// </summary>
        public bool IsTimeOut { get; }
        /// <summary>
        /// To start the module and connect to the remote.
        /// </summary>
        /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
        public void Start();
        /// <summary>
        /// To close the module and close the connection to the remote.
        /// </summary>
        public void Close();
        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <exception cref="InvalidOperationException">Thrown if the module is not connecting, -or- the module mode is not in send mode.</exception>
        public void Send(BaseMsg msg);
        /// <summary>
        /// Receive a earliest unread message.
        /// </summary>
        /// <returns> A earliest unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public BaseMsg Receive();
        /// <summary>
        /// Receive a earliest unread message.
        /// </summary>
        /// <returns> A earliest unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public BaseMsg[] ReceiveAll();
    }
}
