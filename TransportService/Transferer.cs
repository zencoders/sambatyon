using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections;
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
        private ArrayList buffer;
        private string RID;
        private Dictionary<string, float> peerQueue;
        private bool shouldStop;
        private int maxNumber;
        private int nextThread;
        private StreamWriter writer;

        public Transferer(string RID, int begin, int length, Dictionary<string, float> peerQueue, Stream s)
        {
            AppSettingsReader asr = new AppSettingsReader();
            this.poolSize = (int)asr.GetValue("ThreadPoolSize", typeof(int));
            this.threadPool = new ThreadPoolObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                this.threadPool[i] = new ThreadPoolObject();
            }
            this.RID = RID;
            this.peerQueue = peerQueue;
            this.buffer = new ArrayList();
            this.maxNumber = System.Convert.ToInt32(Math.Ceiling((double)(length / ((int)asr.GetValue("ChunkLength", typeof(int))))));
            this.maxNumber -= begin;
            this.worker = new Thread(() => DoWork());
            this.nextThread = 0;
            this.writer = new StreamWriter(s);
        }

        private void GetNextChunk()
        {
            string address = this.GetBestPeer();
            ChunkResponse result = this.GetRemoteChunk(new ChunkRequest(), address);
            this.buffer.Add(result.CID);
            this.writer.Write(result.Payload);            
            //RICALCOLA IL PUNTEGGIO DEL PEER E RIMETTILO IN LISTA
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
            while ( ! this.FullyDownloaded() )
            {
                ThreadPoolObject chunkGetter = this.GetNextThreadInPool();
                chunkGetter.assignAndStart(new ThreadStart(() => GetNextChunk()));
            }
        }

        private ThreadPoolObject GetNextThreadInPool()
        {
            int prevThread = this.nextThread;
            this.nextThread++;
            return this.threadPool[prevThread % this.poolSize];
        }

        private string GetBestPeer()
        {
            string best = this.peerQueue.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            this.peerQueue.Remove(best);
            return best;
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
