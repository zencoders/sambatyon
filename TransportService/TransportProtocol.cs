using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;
using Persistence;
using UdpTransportBinding;
using TransportService.Messages;

namespace TransportService
{
    public delegate void NextArrivedHandler(object o, NextArrivedEventArgs e);

    public class NextArrivedEventArgs
    {

        public int CID { get; set; }

        public byte[] Payload { get; set; }

        public NextArrivedEventArgs(int cID, byte[] payload)
        {
            this.CID = cID;
            this.Payload = payload;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TransportProtocol : ITransportProtocol
    {
        private string RID;
        private bool shouldStop;
        private int maxNumber;
        private int nextChunkToWrite;
        private int chunkLength;
        private int servingBuffer = 0;
        private Dictionary<int, BufferChunk> buffer;
        private event NextArrivedHandler nextArrived;
        private ThreadPool threadPool;
        private Thread worker;
        private StreamWriter writer;
        private Repository trackRepository;
        private Uri myAddress;
        private PeerQueue peerQueue;

        public TransportProtocol(Uri uri, Persistence.Repository trackRepository)
        {
            AppSettingsReader asr = new AppSettingsReader();
            int poolSize = (int)asr.GetValue("ThreadPoolSize", typeof(int));
            this.threadPool = new ThreadPool(poolSize);
            this.worker = new Thread(() => doWork());
            this.nextArrived += new NextArrivedHandler(this.writeOnStream);
            this.chunkLength = ((int)asr.GetValue("ChunkLength", typeof(int)));
            this.myAddress = uri;
            this.trackRepository = trackRepository;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void writeOnStream(Object o, NextArrivedEventArgs e)
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
        private void saveOnBuffer(int CID, byte[] payload)
        {
            this.buffer[CID].Payload = payload;
            this.buffer[CID].ActualCondition = BufferChunk.condition.DOWNLOADED;
            if (CID == this.nextChunkToWrite)
            {
                Console.WriteLine("IN WRITING!");
                nextArrived(new object(), new NextArrivedEventArgs(CID, payload));
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

        private void getNextChunk()
        {
            string address = peerQueue.GetBestPeer();
            int nextChunk = this.nextChunkToGet();
            try
            {
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.DIRTY;
                this.getRemoteChunk(new ChunkRequest(this.RID, nextChunk, myAddress), address);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                this.buffer[nextChunk].ActualCondition = BufferChunk.condition.CLEAN;
            //    this.peerQueue.Remove(address);
                return;
            }
        }

        private void getRemoteChunk(ChunkRequest chkrq, string address)
        {
            ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(address)
            );
            svc.GetChunk(chkrq);
        }

        private void doWork()
        {
            while ( (!this.fullyDownloaded()) && (!this.shouldStop))
            {
            //    Thread.Sleep(1000);
                ThreadPoolObject chunkGetter = this.threadPool.GetNextThreadInPool();
                chunkGetter.AssignAndStart(new ThreadStart(() => getNextChunk()));
            }
/*            Console.WriteLine("FINISHED!");
            for (int i = this.maxNumber - this.buffer.Count(); i < this.maxNumber; i++)
            {
                Console.WriteLine(this.buffer[i].Payload);
            }*/
        }

        private int nextChunkToGet()
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

        private bool fullyDownloaded()
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

        public void Start(string RID, int begin, int length, Dictionary<string, float> peerQueue, Stream s)
        {
            this.RID = RID;
            this.peerQueue = new PeerQueue(peerQueue);
            Console.WriteLine("start");
            this.maxNumber = System.Convert.ToInt32(
                Math.Ceiling((double)(length / this.chunkLength))
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

        public void Stop()
        {
            this.shouldStop = true;
            this.worker.Join();
        }

        public void GetChunk(ChunkRequest chkrq)
        {
            servingBuffer++;
            TrackModel track = new TrackModel();
            RepositoryResponse resp = trackRepository.GetByKey<TrackModel.Track>(chkrq.RID, track);
            if (resp >= 0)
            {
                FileStream fs = new FileStream(track.Filepath, FileMode.Open, FileAccess.Read);
                int limit = (System.Convert.ToInt32(fs.Length) > (chunkLength * (chkrq.CID + 1))) ?  chunkLength * (chkrq.CID + 1) : (System.Convert.ToInt32(fs.Length));
                byte[] data = new byte[limit - (chkrq.CID*chunkLength)];
                fs.Read(data,(chkrq.CID * chunkLength),limit);
                fs.Close();
//                byte[] wantedData = data.Take(chunkLength*chkrq.CID).Skip(chunkLength*(chkrq.CID - 1)).ToArray();
                ChunkResponse chkrs = new ChunkResponse(servingBuffer, chkrq.RID, chkrq.CID, data, myAddress); //SERVING BUFFER SEMPRE 0 PERCHE' NON STIAMO VALUTANDO I PEER
                ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                    new NetUdpBinding(), new EndpointAddress(chkrq.SenderAddress)
                );
                svc.ReturnChunk(chkrs);
            }
            servingBuffer--;
        }

        public void ReturnChunk(ChunkResponse chkrs)
        {
            this.saveOnBuffer(chkrs.CID, chkrs.Payload);
            this.peerQueue.ResetPeer(chkrs.SenderAddress.AbsoluteUri, chkrs.ServingBuffer);
        }
    }
}