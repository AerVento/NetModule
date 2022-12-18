using NetModule.Messages;
using NetModule.Module.Internal.Receiver;
using NetModule.Module.Internal.HeartMsg;
using NetModule.Module.Internal.Stream;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetModule.Module
{
    /// <summary>
    /// A instance of tcp module.
    /// </summary>
    public class TcpModule
    {
        private ModuleStatus status;
        private ModuleMode mode;
        private Socket socket;
        private IPEndPoint remote;
        private HeartMsgModule heartMsgManager;
        private bool isSendingHeartMsgActivated = true;
        private StreamReceiver receiver;

        /// <summary>
        /// A action called when error happened. Often used to print out the exception.
        /// </summary>
        public event Action<Exception> onError;
        
        /// <summary>
        /// How many miliseconds are passed before the remote has time out.
        /// </summary>
        public const int TIME_OUT_TIME = 10000;


        /// <summary>
        /// The remote ip end point.
        /// </summary>
        public IPEndPoint Remote => remote;
        /// <summary>
        /// The count of unread messages.
        /// </summary>
        public Task<int> Count => receiver.GetCount();
        /// <summary>
        /// Whether the remote haven't send message for a time. 
        /// </summary>
        public bool IsTimeOut => heartMsgManager.IsTimeOut;
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
        /// To set the initial remote end point of this module.
        /// </summary>
        /// <param name="ipAddress">The ip address of remote end point.</param>
        /// <param name="port">The port of remote end point.</param>
        /// <param name="mode">The operation mode of module.</param>
        /// <param name="activeHeartMsg">Whether to send heart message. </param>
        public TcpModule(string ipAddress, int port, ModuleMode mode, bool activeHeartMsg)
        {
            remote = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.mode = mode;
            isSendingHeartMsgActivated = activeHeartMsg;
            heartMsgManager = new HeartMsgModule(socket, TIME_OUT_TIME);
            status = ModuleStatus.Initialized;
        }
        /// <summary>
        /// Use a socket which already connected to remote to initialize the module.
        /// </summary>
        /// <param name="connectedSocket">A socket that is already connected to remote.</param>
        /// <param name="mode">The mode of the module.</param>
        /// <param name="activeHeartMsg">Whether to send heart message.</param>
        /// <exception cref="InvalidOperationException"> The socket is not connected. </exception>
        public TcpModule(Socket connectedSocket, ModuleMode mode , bool activeHeartMsg)
        {
            this.mode = mode;
            if (connectedSocket.Connected)
            {
                socket = connectedSocket;
                remote = connectedSocket.RemoteEndPoint as IPEndPoint;
                receiver = new StreamReceiver(new SocketStream(connectedSocket));
                receiver.OnReceived += (msg) => heartMsgManager.Refresh();
                
                isSendingHeartMsgActivated = activeHeartMsg;
                heartMsgManager = new HeartMsgModule(socket, TIME_OUT_TIME);
                
                status = ModuleStatus.Connecting;
            }
            else
            {
                throw new InvalidOperationException("Present socket haven't connected to remote yet.");
            }

        }

  

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="msg">The message to be sent.</param>
        /// <exception cref="InvalidOperationException">Thrown if the module is not connecting, -or- the module mode is not in send mode.</exception>
        public void Send(BaseMsg msg)
        {
            if (CanSend)
            {
                if (status == ModuleStatus.Connecting)
                {
                    //没有启动发送心跳包
                    if (heartMsgManager == null)
                    {
                        //直接异步发送数据
                        Task.Run(() =>
                        {
                            try
                            {
                                socket.Send(msg.GetBytes());
                            }
                            catch (SocketException e)
                            {
                                onError?.Invoke(e);
                                Close();
                            }
                            catch (Exception e)
                            {
                                onError?.Invoke(e);
                            }
                        });
                    }
                    else
                    {
                        //提供的方法会被异步执行
                        heartMsgManager.Lock(() =>
                        {
                            try
                            {
                                socket.Send(msg.GetBytes());
                            }
                            catch (SocketException e)
                            {
                                onError?.Invoke(e);
                                Close();
                            }
                            catch (Exception e)
                            {
                                onError?.Invoke(e);
                            }
                        });
                    }
                }
                else
                    throw new InvalidOperationException("The module is not connecting.");
            }
            else
                throw new InvalidOperationException("The module is not in send mode.");
        }

        
        /// <summary>
        /// Receive a earliest unread message.
        /// </summary>
        /// <returns> A earliest unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public Task<BaseMsg> Receive()
        {
            if (CanReceive && status == ModuleStatus.Connecting)
            {
                return receiver.Receive();
            }
            else
                throw new InvalidOperationException("The module is not in receive mode.");

        }
        /// <summary>
        /// Receive a earliest unread message.
        /// </summary>
        /// <returns> A earliest unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public BaseMsg[] ReceiveAll()
        {
            if (CanReceive && status == ModuleStatus.Connecting)
            {
                return receiver.ReceiveAll();
            }
            else
                throw new InvalidOperationException("The module is not in receive mode.");

        }

        
        /// <summary>
        /// To start the module and connect to the remote.
        /// </summary>
        /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
        /// <exception cref="InvalidOperationException">When module have already started.</exception>
        public void Start()
        {
            try
            {
                switch (status)
                {
                    case ModuleStatus.Initialized:
                        socket.Connect(remote);
                        receiver = new StreamReceiver(new SocketStream(socket));
                        receiver.OnReceived += (msg) => heartMsgManager.Refresh();
                        if (isSendingHeartMsgActivated && heartMsgManager.IsClosed)
                            heartMsgManager.Start();
                        status = ModuleStatus.Connecting;
                        break;
                    case ModuleStatus.Connecting:
                        throw new InvalidOperationException("The module have already started.");
                    case ModuleStatus.Closed:
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        status = ModuleStatus.Initialized;
                        Start();
                        break;
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
                onError?.Invoke(e);
            }
        }

        /// <summary>
        /// To close the module and disconnected to the remote.
        /// </summary>
        /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
        /// <exception cref="InvalidOperationException">When the module haven't started yet.</exception>
        public void Close()
        {
            try
            {
                switch (status)
                {
                    case ModuleStatus.Connecting:
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        heartMsgManager.Close();
                        status = ModuleStatus.Closed;
                        break;
                    default:
                        throw new InvalidOperationException("The module haven't start yet.");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

    }
}
