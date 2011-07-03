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
using log4net;

namespace TransportService
{
    internal delegate void NextArrivedHandler(object o, NextArrivedEventArgs e);

    internal class NextArrivedEventArgs
    {

        public int CID { get; set; }

        public byte[] Payload { get; set; }

        public NextArrivedEventArgs(int cID, byte[] payload)
        {
            this.CID = cID;
            this.Payload = payload;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TransportProtocol : ITransportProtocol
    {
        private string RID;
        private bool shouldStop;
        private int maxNumber;
        private int nextChunkToWrite;
        private int chunkLength;
        private int servingBuffer = 0;
        private bool started = false;
        private Dictionary<int, BufferChunk> buffer;
        private event NextArrivedHandler nextArrived;
        private ThreadPool threadPool;
        private Thread worker;
        private Stream writer;
        private Repository trackRepository;
        private Uri myAddress;
        private PeerQueue peerQueue;
        private static readonly ILog log = LogManager.GetLogger(typeof(TransportProtocol));

        public int ChunckLength
        {
            get
            {
                return chunkLength;
            }
        }

        public TransportProtocol(Uri uri, Persistence.Repository trackRepository)
        {
            AppSettingsReader asr = new AppSettingsReader();
            int poolSize = (int)asr.GetValue("ThreadPoolSize", typeof(int));
            this.threadPool = new ThreadPool(poolSize);
            this.nextArrived += new NextArrivedHandler(this.writeOnStream);
            this.chunkLength = ((int)asr.GetValue("ChunkLength", typeof(int)));
            this.myAddress = uri;
            this.trackRepository = trackRepository;
            log.Info("Initialized Transport Layer with " + poolSize + " worker threads");
        }

        private void writeOnStream(Object o, NextArrivedEventArgs e)
        {
            log.Debug("Writing Chunk " + this.nextChunkToWrite + " to the stream");
            try
            {
                this.writer.Write(e.Payload, 0, e.Payload.Length);
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);
            }
            log.Debug("Written Chunk " + this.nextChunkToWrite + " to the stream");
            this.nextChunkToWrite++;
            while (this.buffer.ContainsKey(this.nextChunkToWrite) && this.buffer[this.nextChunkToWrite].ActualCondition == BufferChunk.condition.DOWNLOADED)
            {
                log.Debug("Writing Chunk " + this.nextChunkToWrite + " to the stream");
                this.writer.Write(this.buffer[this.nextChunkToWrite].Payload, 0, this.buffer[this.nextChunkToWrite].Payload.Length);
                log.Debug("Written Chunk " + this.nextChunkToWrite + " to the stream");
                this.nextChunkToWrite++;
            }
        }

        private void saveOnBuffer(int CID, byte[] payload)
        {
            this.buffer[CID].Payload = payload;
            this.buffer[CID].ActualCondition = BufferChunk.condition.DOWNLOADED;
            if (CID == this.nextChunkToWrite)
            {
                log.Debug("Expected chunk arrived!");
                nextArrived(new object(), new NextArrivedEventArgs(CID, payload));
            }
            else
            {
                log.Debug("Arrived: " + CID + "; Expected: " + this.nextChunkToWrite + " => buffering");
                if (CID > this.nextChunkToWrite)
                {
                    try
                    {
                        this.buffer.AsParallel().Where(
                            Elem => (Elem.Key < CID &&
                                    Elem.Value.ActualCondition == BufferChunk.condition.DIRTY)
                            ).ForAll(
                            Elem => Elem.Value.ActualCondition = BufferChunk.condition.CLEAN
                        );
                    }
                    catch (Exception)
                    {
                        log.Debug("Nothing to do over list");
                    }
                }
            }
        }

        private void getNextChunk()
        {
            string address = peerQueue.GetBestPeer();
            int nextChunk = this.nextChunkToGet();
            if (nextChunk != -1)
            {
                try
                {
                    log.Info("Putting chunk " + nextChunk + " in dirty state.");
                    this.buffer[nextChunk].ActualCondition = BufferChunk.condition.DIRTY;
                    this.getRemoteChunk(new ChunkRequest(this.RID, nextChunk, myAddress), address);
                }
                catch (Exception e)
                {
                    log.Error("Caught exception during chunk getting", e);
                    log.Info("Putting chunk " + nextChunk + " in clean state.");
                    this.buffer[nextChunk].ActualCondition = BufferChunk.condition.CLEAN;
                    return;
                }
            }
            else
            {
                log.Info("No clean chunks for the moment!");
                Thread.Sleep(1000);
            }
        }

        private void getRemoteChunk(ChunkRequest chkrq, string address)
        {
            if (address != this.myAddress.ToString())
            {
                log.Debug("Requesting for chunk " + chkrq.CID + " to remote peer: " + address);
                peerQueue[address].GetChunk(chkrq);
            }
            else
            {
                this.GetChunk(chkrq);
            }
        }

        private void doWork()
        {
            log.Info("Running getter!");
            while ( (!this.fullyDownloaded()) && (!this.shouldStop))
            {
                ThreadPoolObject chunkGetter = this.threadPool.GetNextThreadInPool();
                chunkGetter.AssignAndStart(new Action(getNextChunk));
                Thread.Sleep(100);
            }
        }

        private int nextChunkToGet()
        {
            int best = -1;
            try{
            best = this.buffer.AsParallel().Where(
                Elem => Elem.Value.ActualCondition == BufferChunk.condition.CLEAN
                ).Aggregate((l, r) => l.Key < r.Key ? l : r).Key;
            } catch(Exception){
                log.Error("No clean chunks to get");
/*                int a = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.CLEAN
                    ).Count();
                int b = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DOWNLOADED
                    ).Count();
                int c = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DIRTY
                    ).Count();
                log.Info("Downloaded " + this.buffer.Count + " over " + this.maxNumber + "(clean: "+a+" dirty "+c+" downloaded "+b+" )");*/
            }
            return best;
        }

        private bool fullyDownloaded()
        {
            int downloaded = this.buffer.AsParallel().Where(Elem => Elem.Value.ActualCondition == BufferChunk.condition.DOWNLOADED
                ).Count();
            if (downloaded >= this.buffer.Count())
            {
                this.started = false;
                log.Info("Fully downloaded. Yuppiyaya!!!!");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReStart()
        {
            this.started = true;
            this.worker = new Thread(() => doWork());
            this.worker.Start();
        }

        public void Start(string RID, int begin, long length, Dictionary<string, float> peerQueue, Stream s)
        {
            this.worker = new Thread(() => doWork());
            this.RID = RID;
            this.peerQueue = new PeerQueue(peerQueue);
            log.Info("Starting download of Resource identified by " + RID + " from " + begin + " with a len of " + length);
            this.maxNumber = System.Convert.ToInt32(
                Math.Floor((double)(length / (this.chunkLength * 1024)))
            );
            this.maxNumber -= begin;
            this.writer = s;
            this.buffer = new Dictionary<int, BufferChunk>();
            for (int i = begin; i < this.maxNumber; i++)
            {
                this.buffer[i] = new BufferChunk();
            }
            this.nextChunkToWrite = begin;
            this.started = true;
            this.worker.Start();
        }

        public void Stop()
        {
            if (started)
            {
                log.Info("Stopping download.");
                this.shouldStop = true;
                log.Info("Joining Worker thread...");
                this.worker.Join();
                log.Info("Stopped!");
                this.shouldStop = false;
                this.started = false;
            }
        }

        public void GetChunk(ChunkRequest chkrq)
        {
            log.Info("Received request to send chunk!");
            servingBuffer++;
            TrackModel track = new TrackModel();
            RepositoryResponse resp = trackRepository.GetByKey<TrackModel.Track>(chkrq.RID, track);
            log.Debug("Searching track " + track + " in repository");
            if (resp >= 0)
            {
                log.Debug("Track found! Extracting chunk.");
                byte[] data;
                using (FileStream fs = new FileStream(track.Filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int limit = (System.Convert.ToInt32(fs.Length) > (chunkLength * 1024 * (chkrq.CID + 1))) ? chunkLength * 1024 * (chkrq.CID + 1) : (System.Convert.ToInt32(fs.Length));
                    int begin = chkrq.CID * chunkLength * 1024;
                    data = new byte[limit - begin];
                    Console.WriteLine("Reading chunk " + chkrq.CID + " (" + (chkrq.CID * chunkLength * 1024) + " => " + limit + ")");
                    fs.Seek(begin, SeekOrigin.Begin);
                    fs.Read(data, 0, (limit - begin));
                    fs.Close();
                }
                ChunkResponse chkrs = new ChunkResponse(servingBuffer, chkrq.RID, chkrq.CID, data, myAddress);
                if (chkrq.SenderAddress != myAddress)
                {
                    ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                        new NetUdpBinding(), new EndpointAddress(chkrq.SenderAddress)
                    );
                    svc.ReturnChunk(chkrs);
                }
                else
                {
                    this.ReturnChunk(chkrs);
                }
                log.Debug("Chunk sent to " + chkrq.SenderAddress);
            }
            servingBuffer--;
        }

        public void ReturnChunk(ChunkResponse chkrs)
        {
            log.Info("Arrived chunk from peer!");
            this.saveOnBuffer(chkrs.CID, chkrs.Payload);
            this.peerQueue.ResetPeer(chkrs.SenderAddress.AbsoluteUri, chkrs.ServingBuffer);
        }
    }
}