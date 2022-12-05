using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetModule.Module.Internal.Stream
{
    internal class SocketStream:IStream
    {
        public const int BUFFER_SIZE = 100*1024;
        private Socket socket;

        public SocketStream(Socket connectedSocket)
        {
            this.socket = connectedSocket;
        }

        public int Available => socket.Available;

        public int Read()
        {
            if (socket.Available > 0)
            {
                byte[] b = new byte[1];
                socket.Receive(b, 1, SocketFlags.None);
                return b[0];
            }
            else
                return -1;
        }

        public byte[] ReadAll()
        {
            if (socket.Available > 0)
            {
                byte[] b = new byte[socket.Available];
                socket.Receive(b);
                return b;
            }
            else
                return new byte[0];
        }
    }
}
