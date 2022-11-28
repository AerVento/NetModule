using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetModule.Module.HeartMsg
{
    public class HeartMsgModule
    {
        private Socket remoteSocket;
        private DateTime lastReceivedTime = DateTime.Now;
        private int milisecondsTimeOut;
        private const int FREQUENCY = 5000;
        /// <summary>
        /// Whether the remote is time out.
        /// </summary>
        public bool IsTimeOut => (DateTime.Now - lastReceivedTime).TotalMilliseconds > milisecondsTimeOut;
        /// <summary>
        /// If the thread of sending message is closed.
        /// </summary>
        public bool IsClosed { get; private set; } = true;
        public HeartMsgModule(Socket socket, int milisecondsTimeOut)
        {
            remoteSocket = socket;
            this.milisecondsTimeOut = milisecondsTimeOut;
        }
        
        /// <summary>
        /// The client received a message and to refresh the remote status.
        /// </summary>
        public void Refresh()
        {
            lastReceivedTime = DateTime.Now;
        }

        private object lockObject = new object();
        /// <summary>
        /// Lock the heart message module, and pass a call to present the process after locked.
        /// The call will be executed in aysnc.
        /// </summary>
        /// <param name="call">A process call.</param>
        public void Lock(Action call)
        {
            object obj = lockObject;
            if(call != null)
            {
                Task.Run(() =>
                {
                    lock (lockObject)
                    {
                        call.Invoke();
                    }
                });
            }
        }
        private Thread sendingThread;
        /// <summary>
        /// To start the thread of sending heart message. Not necessary to call if only to receive heart messages.
        /// </summary>
        public void Start()
        {
            if(sendingThread == null || !sendingThread.IsAlive)
            {
                sendingThread = new Thread(RealSendThread);
                sendingThread.Start();
            }
            IsClosed = false;
        }
        /// <summary>
        /// Close the module and the module cannot be reused.
        /// </summary>
        public void Close()
        {
            IsClosed = true;
        }

        private void RealSendThread(object obj)
        {
            while (!IsClosed && remoteSocket.Connected)
            {
                object tmp = lockObject;
                lock (lockObject)
                {
                    remoteSocket.Send(new HeartMsg().GetBytes());
                }
                Thread.Sleep(FREQUENCY);
            }
        }
    }
}
