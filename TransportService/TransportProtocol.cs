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

    public class TransportProtocol : ITransportProtocol
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
        private AutoResetEvent peerQueueNotEmpty = new AutoResetEvent(true);

        public TransportProtocol()
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
            Console.WriteLine("WRITTEN");
            while(this.buffer.ContainsKey(this.nextChunkToWrite) && this.buffer[this.nextChunkToWrite].ActualCondition == BufferChunk.condition.DOWNLOADED){
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
                Console.WriteLine("IN WRITING!");
                NextArrived(new object(), new NextArrivedEventArgs(CID, payload));
            }
            else
            {
                Console.WriteLine("ARRIVED: " + CID + "; ATTENDING: " + this.nextChunkToWrite);
                if (CID > this.nextChunkToWrite)
                {
                    try
                    {
                        this.buffer.AsParallel().Where(
                            Elem => Elem.Key < this.nextChunkToWrite
                            ).Where(
                            Elem => Elem.Value.ActualCondition == BufferChunk.condition.DIRTY
                            ).ForAll(
                            Elem => Elem.Value.ActualCondition = BufferChunk.condition.CLEAN
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Nothing to do over list");
                    }
                }
            }
        }

        private void GetNextChunk()
        {
            string address = this.GetBestPeer();
            int nextChunk = this.NextChunkToGet();
            try
            {
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.DIRTY;
                this.GetRemoteChunk(new ChunkRequest(this.RID, nextChunk), address);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.CLEAN;
            //    this.peerQueue.Remove(address);
                return;
            }
            //RICALCOLA IL PUNTEGGIO DEL PEER E AGGIORNALO
            //SIMULO IL RICALCOLO
            this.peerQueue.Add(address, 10);
            this.peerQueueNotEmpty.Set();
        }

        private void GetRemoteChunk(ChunkRequest chkrq, string address)
        {
            ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                new NetTcpBinding(), new EndpointAddress(address)
            );
            svc.GetChunk(chkrq);
        }

        private void DoWork()
        {
            while ( (!this.FullyDownloaded()) && (!this.shouldStop))
            {
            //    Thread.Sleep(1000);
                ThreadPoolObject chunkGetter = this.GetNextThreadInPool();
                chunkGetter.assignAndStart(new ThreadStart(() => GetNextChunk()));
            }
            Console.WriteLine("FINISHED!");
            for (int i = this.maxNumber - this.buffer.Count(); i < this.maxNumber; i++)
            {
                Console.WriteLine(this.buffer[i].Payload);
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
            int best = -1;
            try{
            best = this.buffer.AsParallel().Where(
                Elem => Elem.Value.ActualCondition == BufferChunk.condition.CLEAN
                ).Aggregate((l, r) => l.Key < r.Key ? l : r).Key;
            } catch(Exception e){
                int a = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.CLEAN
                    ).Count();
                int b = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DOWNLOADED
                    ).Count();
                int c = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DIRTY
                    ).Count();
                Console.WriteLine("BUFFER LENGTH " + this.buffer.Count());
                Console.WriteLine("MAX LENGTH " + this.maxNumber);
                Console.WriteLine("CLEAN " + a);
                Console.WriteLine("DOWNLOADED " + b);
                Console.WriteLine("DIRTY " + c);
            }
            return best;
        }

        private string GetBestPeer()
        {
            if (this.peerQueue.Count() <= 0)
            {
                this.peerQueueNotEmpty.WaitOne();
            }
            this.peerQueueNotEmpty.Reset();
            Console.WriteLine("getbestpeer");
            Console.WriteLine(this.peerQueue.Count());
            string best = this.peerQueue.AsParallel().Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            this.peerQueue.Remove(best);
            if (this.peerQueue.Count > 0)
            {
                this.peerQueueNotEmpty.Set();
            }
            return best;
        }

        private bool FullyDownloaded()
        {
            int downloaded = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DOWNLOADED
                ).Count();
            if (downloaded >= this.buffer.Count())
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
            Console.WriteLine("start");
            Console.WriteLine(this.peerQueue.Count());
            this.maxNumber = System.Convert.ToInt32(
                Math.Ceiling((double)(length / ((int)asr.GetValue("ChunkLength", typeof(int)))))
            );
            this.maxNumber -= begin;
            this.writer = new StreamWriter(s);
            this.buffer = new Dictionary<int,BufferChunk>();
            for (int i = begin; i < this.maxNumber; i++)
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

        public void GetChunk(ChunkRequest chkrq)
        {
            //byte[] a = BitConverter.GetBytes(3678);
            //System.Console.Write("Called");
            //return new ChunkResponse(10, chkrq.RID, chkrq.CID, a);
            //TODO: Implement the method searching in lower persistence level
        }

        public void ReturnChunk(ChunkResponse chkrs)
        {
            this.SaveOnBuffer(chkrs.CID, chkrs.Payload);
        }


    }
}