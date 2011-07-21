/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿using System;
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
    /// <summary>
    /// Delegate type, created to pass particular information on call.
    /// This is used to handle the event referred to the arrive of the particular waited chunk.
    /// </summary>
    /// <param name="o">ND</param>
    /// <param name="e">Argument used to pass information about arrived chunk</param>
    internal delegate void NextArrivedHandler(object o, NextArrivedEventArgs e);

    /// <summary>
    /// Class used to pass information about the payload arrived from the sender peer.
    /// </summary>
    internal class NextArrivedEventArgs
    {
        /// <summary>
        /// Property representing the Identificator of a Chunk.
        /// </summary>
        public int CID { get; set; }

        /// <summary>
        /// Property representing the payload contained in the message received.
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="cID">Chunk id passed</param>
        /// <param name="payload">Payload to transport</param>
        public NextArrivedEventArgs(int cID, byte[] payload)
        {
            this.CID = cID;
            this.Payload = payload;
        }
    }

    /// <summary>
    /// Class implementing the ITransportProtocol. Because it is necessary for the system to have a memory
    /// of the chunk arrived and sent, the service is implemented in Singleton. The use of singleton has a bigger
    /// bad side-effect that exclude the possibility to have more than one method of the singleton class
    /// executing on WCF at the same time. In order to bypass this problem has been activated the multiple
    /// concurrency mode and have been used the system threadpool to execute all interfaces method as delegates.
    /// </summary>
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

        /// <summary>
        /// Property representing the length of a chunk.
        /// </summary>
        public int ChunkLength
        {
            get
            {
                return chunkLength;
            }
        }

        /// <summary>
        /// Constructor of the class. It builds the TransportProtocol sublayer, initializing its common
        /// attributes using the app settings in xml file.
        /// </summary>
        /// <param name="uri">The URI of the transport layer</param>
        /// <param name="trackRepository">The repository used to represent tracks over the network</param>
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


        /// <summary>
        /// Method having a signature that match the one required by delegate to execute.
        /// This method is used to write the newly arrived chunk to the stream. If the chunk
        /// enables other following chunks to been written to the stream the method itself writes the
        /// other chunks to the stream.
        /// </summary>
        /// <param name="o">ND</param>
        /// <param name="e">Arguments regarding the chunk arrived</param>
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
                try
                {
                    this.writer.Write(this.buffer[this.nextChunkToWrite].Payload, 0, this.buffer[this.nextChunkToWrite].Payload.Length);
                }
                catch (Exception ex)
                {
                    log.Debug(ex.Message);
                }
                log.Debug("Written Chunk " + this.nextChunkToWrite + " to the stream");
                this.nextChunkToWrite++;
            }
        }

        /// <summary>
        /// Method used to save the chunk arrived to the buffer inside the object.
        /// If the chunk arrived is the waited chunk, the nextArrived event is released.
        /// If the chunk arrived have an identificator greater to the waited the chunk is buffered and
        /// then all chunks in dirty state between the waited chunk and the arrived chunk is set to CLEAN state.
        /// </summary>
        /// <param name="CID">The Chunk identifier</param>
        /// <param name="payload">The payload contained into the chunk</param>
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

        /// <summary>
        /// Method used to require the next chunk to the network.
        /// It selects the best peer to whom ask the chunk, it calculates the next chunk to get and then
        /// invokes the chunk from the remote peer.
        /// </summary>
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

        /// <summary>
        /// Method used to require the chunk to the remote peer. 
        /// </summary>
        /// <param name="chkrq">The message containing informations about the request</param>
        /// <param name="address">The address of the peer to whom require the chunk</param>
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

        /// <summary>
        /// Method that actively downloads the resource from the network.
        /// This method runs while the file is not completely downloaded or a stop have been requested.
        /// </summary>
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

        /// <summary>
        /// Method used to find the index of the next chunk to get from the network.
        /// </summary>
        /// <returns>The CID of the chunk to find</returns>
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

        /// <summary>
        /// Method used to understand at each step if the download is complete.
        /// The download is complete when the number of chunk in DOWNLOADED state is equal to the size
        /// of buffer.
        /// </summary>
        /// <returns>True if the file is fully downloaded; false otherwise.</returns>
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

        /// <summary>
        /// Method used to restart the download without reinitializing the buffer and other inner structures.
        /// </summary>
        public void ReStart()
        {
            this.started = true;
            this.worker = new Thread(() => doWork());
            this.worker.Start();
        }

        /// <summary>
        /// Start method of the class. This method allows the thread to start the procedure of download of
        /// a remote resource in a separate thread. It initialize all attributes related to the resource.
        /// </summary>
        /// <param name="RID">Identificator of the resource</param>
        /// <param name="begin">Start point from the beginning of the file to begin download</param>
        /// <param name="length">>Total length in bytes of the resource</param>
        /// <param name="peerQueue">PeerQueue of peers containing the resource with a score associated to each peer</param>
        /// <param name="s">Stream where the resource is written</param>
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

        /// <summary>
        /// Method used to stop the download.
        /// </summary>
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

        #region interface
        /// <summary>
        /// Method used to request the download of a chunk from the network. When it is invoked from
        /// a remote peer it gets the chunk from the file and calls back the ReturnChunk method
        /// of the requestor.
        /// </summary>
        /// <param name="chkrq">Message used to pass information about the Chunk requested</param>
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

        /// <summary>
        /// Method used to return chunk to requestors. When it is invoked from a remote peer
        /// it invokes the saveOnBuffer method to store the chunk into the buffer. At the end this
        /// method reset the peer status.
        /// </summary>
        /// <param name="chkrs">Message used to transport the payload of a chunk</param>
        public void ReturnChunk(ChunkResponse chkrs)
        {
            log.Info("Arrived chunk from peer!");
            this.saveOnBuffer(chkrs.CID, chkrs.Payload);
            this.peerQueue.ResetPeer(chkrs.SenderAddress.AbsoluteUri, chkrs.ServingBuffer);
        }
        #endregion
    }
}