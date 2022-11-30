using NetModule.Messages;
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
    public class TcpModule: INetModule
    {
        private ModuleStatus status;
        private ModuleMode mode;
        
        private IPEndPoint remote;
        private Socket socket;
        private HeartMsg.HeartMsgModule heartMsgManager;
        private bool isSendingHeartMsgActivated = true;
        private const int cacheBufferSize = 1024 * 5;
        private byte[] receivingCaches = new byte[cacheBufferSize];
        private int cacheIndex = 0;
        private Queue<BaseMsg> receivedMsg = new Queue<BaseMsg>();

        /// <summary>
        /// A action called when error happened.
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
        /// The count of bytes of unread messages.
        /// </summary>
        public int Available => receivedMsg.Count;
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

            heartMsgManager = new HeartMsg.HeartMsgModule(socket, TIME_OUT_TIME);
            
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
                status = ModuleStatus.Connecting;
            }
            else
            {
                throw new InvalidOperationException("Present socket haven't connected to remote yet.");
            }
            isSendingHeartMsgActivated = activeHeartMsg;

            heartMsgManager = new HeartMsg.HeartMsgModule(socket, TIME_OUT_TIME);
        }

        /// <summary>
        /// Rececive byte array and analyse it to a message instance.
        /// </summary>
        private void SocketReceive()
        {
            try
            {
                int length = socket.Receive(receivingCaches, cacheIndex, cacheBufferSize - cacheIndex, SocketFlags.None);
                int result = AnalysePackages(length + cacheIndex);
                if (result == 0)
                {
                    cacheIndex = 0;
                }
                else
                {
                    //Copy the info which cannot be analysed to the front of array.
                    Array.Copy(receivingCaches, length + cacheIndex - result, receivingCaches, 0, result);

                    cacheIndex = result;
                }
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }
        
        /// <summary>
        /// 处理分包 黏包问题
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>从后往前数未解读的字节长度</returns>
        private int AnalysePackages(int length)
        {
            //A begin index of a single message.
            int index = 0;
            while (index < length)
            {
                //Try get current message length.
                byte[] bytes = new byte[BaseMsg.SIZE_OF_LENGTH];
                for(int i = 0; i < BaseMsg.SIZE_OF_LENGTH; i++)
                {
                    int offset = index + BaseMsg.lengthOffset + i;
                    if(offset > length)
                    {
                        return length - index;
                    }
                    bytes[i] = receivingCaches[offset];
                }
                //Real value of current message length.
                int lengthValue = BitConverter.ToInt32(bytes);
                if (lengthValue > length - index) //The message isn't complete.
                    return length - index;

                BaseMsg msg = BaseMsg.GetInstance(receivingCaches, index, lengthValue);
                
                if(msg is HeartMsg.HeartMsg)//tcp模块专用心跳包，检测到就直接分析掉
                {
                    heartMsgManager.Refresh();
                }
                else
                {
                    receivedMsg.Enqueue(msg);
                    heartMsgManager.Refresh();
                }
                index += lengthValue;
            }
            return 0;
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

        private bool taskFlag = false;//if there is already a anaylsing task is executing.
        /// <summary>
        /// To turn message cache to message.
        /// </summary>
        private void RefreshMsg()
        {
            taskFlag = true;
            while (socket.Available > 0)
                SocketReceive();
            taskFlag = false;
        }
        /// <summary>
        /// Receive a earliest unread message.
        /// </summary>
        /// <returns> A earliest unread message, -or- null if there's no unread message.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the module is not in receive mode.</exception>
        public BaseMsg Receive()
        {
            if (CanReceive)
            {
                if (receivedMsg.Count > 0)
                {
                    if (socket.Available > 0 && taskFlag == false)
                        Task.Run(RefreshMsg);

                    return receivedMsg.Dequeue();
                }
                else if(socket.Available > 0 && taskFlag == false)
                {
                    Task<BaseMsg> receiveOne = new Task<BaseMsg>(() =>
                    {
                        taskFlag = true;
                        while (receivedMsg.Count == 0)
                            SocketReceive();
                        taskFlag = false;
                        return receivedMsg.Dequeue();
                    });
                    receiveOne.Start();
                    receiveOne.ContinueWith((task) =>
                    {
                        RefreshMsg();
                    });
                    receiveOne.Wait();
                    return receiveOne.Result;
                }
                else
                {
                    return null;
                }
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
            if (CanReceive)
            {
                if (receivedMsg.Count > 0)
                {
                    BaseMsg[] msgs = new BaseMsg[receivedMsg.Count];
                    int i = 0;
                    while (receivedMsg.Count > 0)
                    {
                        msgs[i] = receivedMsg.Dequeue();
                        i++;
                    }
                    return msgs;
                }
                else if (socket.Available > 0 && taskFlag == false)
                {
                    Task.Run(RefreshMsg);
                    return null;
                }
                else
                    return null;
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
            if (status == ModuleStatus.Connecting)
                throw new InvalidOperationException("The module have already started.");
            try
            {
                if (status == ModuleStatus.Closed)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    status = ModuleStatus.Initialized;
                }
                if (status == ModuleStatus.Initialized)
                {
                    socket.Connect(remote);
                    //if(mode == ModuleMode.Receive)
                    //    ThreadPool.QueueUserWorkItem(SocketReceive);
                    status = ModuleStatus.Connecting;
                }
                if (isSendingHeartMsgActivated && heartMsgManager.IsClosed)
                {
                    heartMsgManager.Start();
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
            if (status != ModuleStatus.Connecting)
                throw new InvalidOperationException("The module haven't start yet.");
            try
            {
                if (status == ModuleStatus.Connecting)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket = null;
                    status = ModuleStatus.Closed;
                }
                if (isSendingHeartMsgActivated && !heartMsgManager.IsClosed)
                    heartMsgManager.Close();
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }

    }
}
