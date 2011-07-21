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
using System.Collections.Generic;
using System.ServiceModel;
using TransportService;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using Kademlia;
using System.ServiceModel.Description;
using Persistence;
using System.Threading.Tasks;
using log4net;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PeerLibrary
{
    /// <summary>
    /// Class implementing the IPeer interface. Because it is necessary for the system to have a memory
    /// of the status the service is implemented in Singleton. The use of singleton has a bigger
    /// bad side-effect that exclude the possibility to have more than one method of the singleton class
    /// executing on WCF at the same time. In order to bypass this problem has been activated the multiple
    /// concurrency mode and have been used the system threadpool to execute all interfaces method as delegates.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Peer : IDisposable, IPeer
    {
        private TransportProtocol transportLayer;
        private static readonly ILog log = LogManager.GetLogger(typeof(Peer));
        private Dht kademliaLayer;
        private Stream localStream;
        private ServiceHost[] svcHosts = new ServiceHost[3];
        private bool single;
        private string btpNode;
        private Persistence.Repository trackRep;
        private string peerAddress;
        private Uri transportAddress;
        private Uri kademliaAddress;

        #region Properties
        /// <summary>
        /// Key-value representation of option that can be passed to the peer.
        /// </summary>
        public Dictionary<string, string> ConfOptions {get; set;}

        /// <summary>
        /// Property that represents the length of a chunk in the network.
        /// </summary>
        public int ChunkLength
        {
            get
            {
                if (transportLayer != null)
                {
                    return transportLayer.ChunkLength;
                }
                else
                {
                    return 0;
                }
            }
        }        
        #endregion

        /// <summary>
        /// Peer Constructor. It initialize configuration according to the options passed, initialize the
        /// local stream, creates the databases and resolves the IP address using Dns class.
        /// </summary>
        /// <param name="single">Indicates if the peer have to run kademlia layer in single bootstrap</param>
        /// <param name="btpNode">Address of the suggested bootstrap node.</param>
        public Peer(bool single = false, string btpNode = "")
        {
            log.Debug("Initializing peer structure");
            this.ConfOptions = new Dictionary<string, string>();
            this.ConfOptions["udpPort"] = PeerLibrary.Properties.Settings.Default.udpPort;
            this.ConfOptions["kadPort"] = PeerLibrary.Properties.Settings.Default.kademliaPort;
            this.localStream = new MemoryStream();
            this.single = single;
            this.btpNode = btpNode;
            AppSettingsReader asr = new AppSettingsReader();
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(new { data_dir = (string)asr.GetValue("TrackRepository", typeof(string)) });
            this.trackRep = Persistence.RepositoryFactory.GetRepositoryInstance("Raven", conf);
//            this.peerAddress = "127.0.0.1";
            IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] listaIP = IPHost.AddressList;
            foreach (IPAddress ip in listaIP)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.peerAddress = ip.ToString();
                    break;
                }
            }
        }

        /// <summary>
        /// Method used to construct all local addresses according to configuration.
        /// </summary>
        private void calculateAddresses()
        {
            string udpPort = this.ConfOptions["udpPort"];
            string kademliaPort = this.ConfOptions["kadPort"];
            this.transportAddress = new Uri("soap.udp://" + this.peerAddress + ":" + udpPort + "/transport_protocol");
            this.kademliaAddress = new Uri("soap.udp://" + this.peerAddress + ":" + kademliaPort + "/kademlia");
        }

        /// <summary>
        /// Method used to run layers (kademlia, transport, interface). Each layer is ran in a separate
        /// thread in order to allow to register the thread hosting the service with a referrer thread id
        /// (in global threadpool) that is different from the mai thread.
        /// The method will wait each layer to have completely finished booting before passing to another
        /// layer.
        /// </summary>
        /// <param name="withoutInterface">indicates whether to start or not the peer interface layer.</param>
        public void RunLayers(bool withoutInterface=false)
        {
            log.Debug("Running layers...");
            this.calculateAddresses();
            Exception kadStartExp = null;
            Exception transportStartExp = null;
            Exception interfaceStartExp = null;
            Thread kadThread = kadThread = new Thread(new ThreadStart(() => {
                try {
                    this.runKademliaLayer(single, btpNode, ref svcHosts[0]);
                } catch (Exception e)
                {
                    kadStartExp = e;
                }
            }));
            Thread transportThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    this.runTransportLayer(ref svcHosts[1]);
                }
                catch (Exception e)
                {
                    transportStartExp = e;
                }
            }));            
            if (!withoutInterface)
            {
                Thread interfaceThread = new Thread(new ThreadStart(() => {
                    try {
                        this.runInterfaceLayer(ref svcHosts[2]);
                    } catch (Exception e)
                    {
                        interfaceStartExp=e;
                    }
                }));
                interfaceThread.Start();
                interfaceThread.Join();
                if (interfaceStartExp!= null)
                {
                    throw interfaceStartExp;
                }
            }            
            kadThread.Start();
            kadThread.Join();
            if (kadStartExp != null)
            {
                throw kadStartExp;
            }
            transportThread.Start();
            transportThread.Join();
            if (transportStartExp !=null)
            {
                throw transportStartExp;
            }
        }

        #region layersInitialization
        /// <summary>
        /// Method (usually runned like a thread) that privately hosts the interface service.
        /// </summary>
        /// <param name="svcHost">a service host object where is stored newly initalized host.</param>
        private void runInterfaceLayer(ref ServiceHost svcHost)
        {
            log.Info("Running Interface Layer.");
            ServiceHost host = new ServiceHost(this);
            try
            {
                host.Open();
            }
            catch (AddressAlreadyInUseException aaiue)
            {
                log.Error("Unable to Connect as a Server because there is already one on this machine", aaiue);
            }
            foreach (Uri uri in host.BaseAddresses)
            {
                log.Info(uri.ToString());
            }
            svcHost = host;
        }

        /// <summary>
        /// Method that runs the transport layer
        /// </summary>
        /// <param name="svcHost">a service host object where is stored newly initalized host.</param>
        private void runTransportLayer(ref ServiceHost svcHost)
        {
            log.Info("Running Transport Layer.");
            TransportProtocol tsp = new TransportProtocol(transportAddress, this.trackRep);
            this.transportLayer = tsp;
            ServiceHost host = new ServiceHost(tsp, transportAddress);
            try
            {
                host.Open();
                #region Output dispatchers listening
                foreach (Uri uri in host.BaseAddresses)
                {
                    log.Info(uri.ToString());
                }
                log.Info("Number of dispatchers listening : " + host.ChannelDispatchers.Count);
                foreach (System.ServiceModel.Dispatcher.ChannelDispatcher dispatcher in host.ChannelDispatchers)
                {
                    log.Info(dispatcher.Listener.Uri.ToString());
                }
                #endregion
            }
            catch (AddressAlreadyInUseException aaiue)
            {
                log.Error("Unable to Connect as a Server because there is already one on this machine", aaiue);
                throw aaiue;
            }
            svcHost = host;
        }

        /// <summary>
        /// Method that runs the kademlia layer
        /// </summary>
        /// <param name="single">if true indicates that the kademlia layer have to do a single start</param>
        /// <param name="btpNode">if indicated, it represents the node suggested to do bootstrap</param>
        /// <param name="svcHost">a service host object where is stored newly initalized host.</param>
        private void runKademliaLayer(bool single, string btpNode, ref ServiceHost svcHost)
        {
            log.Info("Running Kademlia layer.");
            
            KademliaNode node = new KademliaNode(new EndpointAddress(kademliaAddress), new EndpointAddress(transportAddress));
            ServiceHost kadHost = new ServiceHost(node, kademliaAddress);
            try
            {
                kadHost.Open();
            }
            catch (AddressAlreadyInUseException aaiue)
            {
                log.Error("Unable to Connect as a Server because there is already one on this machine", aaiue);
                throw aaiue;
            }
            try
            {
                this.kademliaLayer = new Dht(node, single, btpNode);
            }
            catch (FileNotFoundException fnfe)
            {
                log.Error("Unable to load nodes file (nodes.xml)", fnfe);
                throw fnfe;
            }
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            log.Debug("GetAll Response : " + this.trackRep.GetAll(list));
            Parallel.ForEach(list, t =>
            {
                this.kademliaLayer.Put(t.Filename);
            });
            svcHost = kadHost;
        }
        #endregion

        #region interface
        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        /// <param name="udpPort">String containing the value of the udpPort</param>
        /// <param name="kademliaPort">String containig the value of the kademliaPort</param>
        public void Configure(string udpPort = "-1", string kademliaPort = "-1")
        {
            log.Info("Reconfiguring peer with " + (udpPort != "-1" ? "udpPort=" + udpPort : "") + (kademliaPort != "-1" ? "kademliaPort=" + kademliaPort : ""));
            if (udpPort != "-1")
            {
                PeerLibrary.Properties.Settings.Default.udpPort = udpPort;
            }
            if (kademliaPort != "-1")
            {
                PeerLibrary.Properties.Settings.Default.kademliaPort = kademliaPort;
            }
            PeerLibrary.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        /// <returns>The stream to use</returns>
        public Stream ConnectToStream()
        {
            log.Info("Returning stream to requestor.");
            return this.localStream;
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        public void RestartFlow()
        {
            if (transportLayer == null)
            {
                Thread transportThread = new Thread(new ThreadStart(() => this.runTransportLayer(ref svcHosts[1])));
                transportThread.Start();
                transportThread.Join();
            }
            this.transportLayer.ReStart();
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        /// <param name="RID">The resource identifier</param>
        /// <param name="begin">The begin point from the head of a file to start download</param>
        /// <param name="length">The total length of file</param>
        /// <param name="nodes">Nodes with associated score that are used to download a file from the network</param>
        public void GetFlow(string RID, int begin, long length, Dictionary<string, float> nodes)
        {
            this.GetFlow(RID, begin, length, nodes, null);
        }

        /// <summary>
        /// Overload of the previous method with an extern stream passed.
        /// </summary>
        /// <param name="RID">The resource identifier</param>
        /// <param name="begin">The begin point from the head of a file to start download</param>
        /// <param name="length">The total length of file</param>
        /// <param name="nodes">Nodes with associated score that are used to download a file from the network</param>
        /// <param name="stream">The stream to use</param>
        public void GetFlow(string RID, int begin, long length, Dictionary<string, float> nodes, Stream stream = null)
        {
            log.Info("Beginning to get flow from the network");
            Stream handlingStream = stream;
            if (handlingStream == null)
            {
                handlingStream = this.localStream;
            }
            if (transportLayer == null)
            {
                Thread transportThread = new Thread(new ThreadStart(() => this.runTransportLayer(ref svcHosts[1])));
                transportThread.Start();
                transportThread.Join();
            }
            this.transportLayer.Start(RID, begin, length, nodes, handlingStream);
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        public void StopFlow()
        {
            log.Info("Stop flow.");
            if (this.transportLayer != null)
            {
                this.transportLayer.Stop();
            }
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        /// <param name="filename">filename of the file to download</param>
        /// <returns>true if the filename have been store; false otherwise</returns>
        public bool StoreFile(string filename)
        {
            log.Info("Storing file:" + filename);
            TrackModel track = new TrackModel(filename);
            TrackModel sameTk = new TrackModel();
            this.trackRep.GetByKey<TrackModel.Track>(track.GetAsDatabaseType().Id, sameTk);
            if ((sameTk != null) && (sameTk.GetAsDatabaseType().Id == track.GetAsDatabaseType().Id))
            {
                log.Warn("Unable to store duplicate file "+filename+" !");
                return false;                
            }
            else
            {
                this.trackRep.Save(track);
                this.kademliaLayer.Put(filename);
                return true;
            }
        }

        /// <summary>
        /// <see cref="PeerLibrary.IPeer"/>
        /// </summary>
        /// <param name="queryString">the querystring used to search</param>
        /// <returns>A list of found resources</returns>
        public IList<KademliaResource> SearchFor(string queryString)
        {
            return this.kademliaLayer.GetAll(queryString);
        }

        /// <summary>
        /// Method used to get all stored tracks
        /// </summary>
        /// <returns>a list of tracks</returns>
        public IList<TrackModel.Track> GetAllTracks()
        {
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            this.trackRep.GetAll(list);
            return list;
        }
        #endregion

        /// <summary>
        /// Method created to implement the IDisposable interface and used to close all service hosts
        /// already running.
        /// </summary>
        public void Dispose()
        {
            log.Info("Disposing Peer");
            foreach (ServiceHost svcHost in this.svcHosts)
            {
                if(svcHost != null)
                    svcHost.Close();
            }
            this.trackRep.Dispose();
        }
    }
}