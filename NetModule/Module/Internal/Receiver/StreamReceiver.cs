using NetModule.Messages;
using NetModule.Module.Internal.Stream;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NetModule.Module.Internal.Receiver
{
    internal class StreamReceiver : IReceiver
    {
        private const int cacheBufferSize = 5 * 1024;
        private IStream stream;
        private byte[] receivingCaches = new byte[cacheBufferSize];
        private int cacheIndex = 0;
        private Queue<BaseMsg> receivedMsgs = new Queue<BaseMsg>();
        public StreamReceiver(IStream stream)
        {
            this.stream = stream;
        }
        private void ReadBytes()
        { 
            int length = stream.Available;
            if (length == 0)
                return;
            stream.ReadAll().CopyTo(receivingCaches, cacheIndex);
            int result = AnalyseBytes(length + cacheIndex);
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
        private int AnalyseBytes(int length)
        {
            //A begin index of a single message.
            int index = 0;
            while (index < length)
            {
                //Try get current message length.
                byte[] bytes = new byte[BaseMsg.SIZE_OF_LENGTH];
                for (int i = 0; i < BaseMsg.SIZE_OF_LENGTH; i++)
                {
                    int offset = index + BaseMsg.lengthOffset + i;
                    if (offset > length)
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

                receivedMsgs.Enqueue(msg);
                index += lengthValue;
            }
            return 0;
        }
        private Task readTask;//可能有多个线程同时启动ReadBytes,标识着唯一的一个task
        private readonly object lockObject = new object();
        private Task StartRead()
        {
            lock (lockObject)
            {
                if (readTask != null)
                    return readTask;
                readTask = new Task(ReadBytes);
                readTask.ContinueWith((task) =>
                {
                    readTask = null;
                });
                readTask.Start();
                return readTask;
            }
        }
        public async Task<int> GetCount()
        {
            if (stream.Available > 0)
            {
                await StartRead();
            }
            return receivedMsgs.Count;
        }

        public async Task<BaseMsg> Receive()
        {
            if (receivedMsgs.Count > 0)
            {
                if (stream.Available > 0)
                    StartRead();
                lock (receivedMsgs)
                {
                    return receivedMsgs.Dequeue();
                }
            }
            else if (stream.Available > 0)
            {
                return await Task.Run(() =>
                {
                    while (receivedMsgs.Count == 0)
                    {
                        StartRead().Wait();
                    }
                    lock (receivedMsgs)
                    {
                        return receivedMsgs.Dequeue();
                    }
                });
            }
            else
                return new NullMsg();
        }

        public BaseMsg[] ReceiveAll()
        {
            if (receivedMsgs.Count > 0)
            {
                lock (receivedMsgs)
                {
                    BaseMsg[] msgs = new BaseMsg[receivedMsgs.Count];
                    int index = 0;
                    while(receivedMsgs.Count > 0)
                    {
                        msgs[index] = receivedMsgs.Dequeue();
                        index++;
                    }
                    return msgs;
                }
            }
            if (stream.Available > 0)
            {
                StartRead();
            }
            return new NullMsg();
        }
    }
}
