using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetModule.Module.Internal.Dgram
{
    internal class SocketDgram : IDgram
    {
        private UdpClient udpClient;
        private Queue<byte[]> dgrams = new Queue<byte[]>();

        public SocketDgram(IPEndPoint local, IPEndPoint remote)
        {
            this.udpClient =  new UdpClient(local);
            Thread thread = new Thread(() =>
            {
                while (udpClient.Client.Connected)
                {
                    if (udpClient.Available > 0)
                    {
                        byte[] arr = udpClient.Receive(ref remote);
                        OnReceivedDgram(arr);
                    }
                    else
                        Thread.Sleep(RefreshTime);
                }
            });
        }
        /// <summary>
        /// The miliseconds time distance between searching for unread dgram in socket.
        /// </summary>
        public int RefreshTime { get; set; }
        public int Count => throw new NotImplementedException();

        private void OnReceivedDgram(byte[] dgram)
        {
            dgrams.Enqueue(dgram);
        }

        public byte[] Read()
        {
            lock (dgrams)
            {
                return dgrams.Dequeue();
            }
        }

        public List<byte[]> ReadAll()
        {
            lock (dgrams)
            {
                List<byte[]> list = new List<byte[]>(dgrams.Count);
                while (dgrams.Count > 0)
                {
                    list.Add(dgrams.Dequeue());
                }
                return list;
            }
        }
    }
}
