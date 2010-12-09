using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Threading;

namespace TransportService
{
    public class Transferer
    {
        private int poolSize;
        private ThreadPoolObject[] threadPool;
        private Thread worker;
        private List<int> buffer;
        private string RID;
        private int length;
        private Dictionary<string, float> peerQueue;
        private Stream s;
        private bool shouldStop;
        private int maxNumber;
        private int next;

        public Transferer(string RID, int length, Dictionary<string, float> peerQueue, Stream s)
        {
            AppSettingsReader asr = new AppSettingsReader();
            this.poolSize = (int)asr.GetValue("ThreadPoolSize", typeof(int));
            this.threadPool = new ThreadPoolObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                this.threadPool[i] = new ThreadPoolObject();
            }
            this.RID = RID;
            this.length = length;
            this.peerQueue = peerQueue;
            this.s = s;
            this.buffer = new List<int>();
            this.maxNumber = System.Convert.ToInt32(Math.Ceiling((double)(length / ((int)asr.GetValue("ChunkLength", typeof(int))))));
//            this.buffer = new int[System.Convert.ToInt32(Math.Ceiling((double)(length / ((int)asr.GetValue("ChunkLength", typeof(int))))))];
            this.worker = new Thread(() => DoWork());
            this.next = 0;
        }

        private ChunkResponse GetRemoteChunk(ChunkRequest chkrq, string address)
        {
            ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                new NetTcpBinding(), new EndpointAddress(address)
            );
            ChunkResponse result = svc.GetChunk(chkrq);
            return result;
        }

        private void DoWork()
        {
            StreamWriter sw = new StreamWriter(this.s);
            while ( ! this.FullyDownloaded() )
            {
                ThreadPoolObject chunkGetter = this.GetNext();
                chunkGetter.assignAndStart(new ThreadStart(() => GetRemoteChunk(new ChunkRequest(), "aaa")));
            }
        }

        private ThreadPoolObject GetNext()
        {
            int prev = this.next;
            this.next++;
            return this.threadPool[prev % this.poolSize];
        }

        private bool FullyDownloaded()
        {
            if (this.buffer.Count == this.maxNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void start()
        {
            this.worker.Start();
        }

        public void stop()
        {
            this.shouldStop = true;
            this.worker.Join();
        }
    }
}
