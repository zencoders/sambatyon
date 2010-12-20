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
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace TransportService
{

    public delegate void NextArrivedHandler(object o, NextArrivedEventArgs e);

    public class NextArrivedEventArgs
    {
        private int cID;
        private byte[] payload;

        public int CID
        {
            get { return this.cID; }
            set { this.cID = value; }
        }

        public byte[] Payload
        {
            get { return this.payload; }
            set { this.payload = value; } 
        }

        public NextArrivedEventArgs(int cID, byte[] payload)
        {
            this.cID = cID;
            this.payload = payload;
        }
    }

    public class Transferer
    {
        private Dictionary<int, BufferChunk> buffer;
        private event NextArrivedHandler NextArrived;
        private AppSettingsReader asr;
        private int poolSize;
        private ThreadPoolObject[] threadPool;
        private Thread worker;
        private string RID;
        private Dictionary<string, float> peerQueue;
        private bool shouldStop;
        private int maxNumber;
        private int nextThread;
        private StreamWriter writer;
        private int nextChunkToWrite;

        public Transferer()
        {
            this.asr = new AppSettingsReader();
            this.poolSize = (int)asr.GetValue("ThreadPoolSize", typeof(int));
            this.threadPool = new ThreadPoolObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                this.threadPool[i] = new ThreadPoolObject();
            }
            this.worker = new Thread(() => DoWork());
            this.nextThread = 0;
            this.NextArrived += new NextArrivedHandler(this.WriteOnStream);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WriteOnStream(Object o, NextArrivedEventArgs e)
        {
            this.writer.Write(e.Payload);
            this.nextChunkToWrite++;
            while(this.buffer.ContainsKey(this.nextChunkToWrite)){
                this.writer.Write(this.buffer[this.nextChunkToWrite]);
                this.nextChunkToWrite++;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SaveOnBuffer(int CID, byte[] payload)
        {
            this.buffer[CID].Payload = payload;
            this.buffer[CID].ActualCondition = BufferChunk.condition.DOWNLOADED;
            if (CID == this.nextChunkToWrite)
            {
                NextArrived(new object(), new NextArrivedEventArgs(CID, payload));
            }
        }

        private void GetNextChunk()
        {
            string address = this.GetBestPeer();
            int nextChunk = this.NextChunkToGet();
            ChunkResponse result;
            try
            {
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.DIRTY;
                result = this.GetRemoteChunk(new ChunkRequest(this.RID, nextChunk), address);
            }
            catch
            {
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.CLEAN;
                this.peerQueue.Remove(address);
                return;
            }
            this.SaveOnBuffer(result.CID, result.Payload);
            //RICALCOLA IL PUNTEGGIO DEL PEER E AGGIORNALO
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
            while ( (!this.FullyDownloaded()) && (!this.shouldStop))
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

        private int NextChunkToGet()
        {
            int best = this.buffer.AsParallel().Where(
                Elem => Elem.Value.ActualCondition == BufferChunk.condition.CLEAN
                ).Aggregate((l, r) => l.Key < r.Key ? l : r).Key;
            return best;
        }

        private string GetBestPeer()
        {
            string best = this.peerQueue.AsParallel().Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
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

        public void start(string RID, int begin, int length, Dictionary<string, float> peerQueue, Stream s)
        {
            this.RID = RID;
            this.peerQueue = peerQueue;
            this.maxNumber = System.Convert.ToInt32(
                Math.Ceiling((double)(length / ((int)asr.GetValue("ChunkLength", typeof(int)))))
            );
            this.maxNumber -= begin;
            this.writer = new StreamWriter(s);
            this.buffer = new Dictionary<int,BufferChunk>();
            for (int i = begin; i < length; i++)
            {
                this.buffer[i] = new BufferChunk();
            }
            this.nextChunkToWrite = begin;
            this.worker.Start();
        }

        public void stop()
        {
            this.shouldStop = true;
            this.worker.Join();
        }
    }
}
