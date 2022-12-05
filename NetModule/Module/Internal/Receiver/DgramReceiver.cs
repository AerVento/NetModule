using NetModule.Messages;
using NetModule.Module.Internal.Dgram;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetModule.Module.Internal.Receiver
{
    internal class DgramReceiver : IReceiver
    {
        private IDgram dgram;
        private Queue<BaseMsg> receivedMsgs = new Queue<BaseMsg>();
        public DgramReceiver(IDgram dgram)
        {
            this.dgram = dgram;
        }

        private void ReadBytes()
        {
            int count = dgram.Count;
            if (count == 0)
                return;
            List<byte[]> list = dgram.ReadAll();
            foreach(byte[] arr in list)
            {
                BaseMsg msg = BaseMsg.GetInstance(arr);
                receivedMsgs.Enqueue(msg);
            }
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
            if(dgram.Count > 0)
                await StartRead();
            return receivedMsgs.Count;
        }

        public async Task<BaseMsg> Receive()
        {
            if (receivedMsgs.Count > 0)
            {
                StartRead();
                lock (receivedMsgs)
                {
                    return receivedMsgs.Dequeue();
                }
            }
            else if (dgram.Count > 0)
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
                    while (receivedMsgs.Count > 0)
                    {
                        msgs[index] = receivedMsgs.Dequeue();
                        index++;
                    }
                    return msgs;
                }
            }
            if (dgram.Count > 0)
            {
                StartRead();
            }
            return new NullMsg();
        }
    }
}
