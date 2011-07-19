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
        public Dictionary<string, string> ConfOptions {get; set;}
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

        private void calculateAddresses()
        {
            string udpPort = this.ConfOptions["udpPort"];
            string kademliaPort = this.ConfOptions["kadPort"];
            this.transportAddress = new Uri("soap.udp://" + this.peerAddress + ":" + udpPort + "/transport_protocol");
            this.kademliaAddress = new Uri("soap.udp://" + this.peerAddress + ":" + kademliaPort + "/kademlia");
        }

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

        public Stream ConnectToStream()
        {
            log.Info("Returning stream to requestor.");
            return this.localStream;
        }

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

        public void GetFlow(string RID, int begin, long length, Dictionary<string, float> nodes)
        {
            this.GetFlow(RID, begin, length, nodes, null);
        }

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

        public void StopFlow()
        {
            log.Info("Stop flow.");
            if (this.transportLayer != null)
            {
                this.transportLayer.Stop();
            }
        }

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

        public IList<KademliaResource> SearchFor(string queryString)
        {
            return this.kademliaLayer.GetAll(queryString);
        }

        public IList<TrackModel.Track> GetAllTracks()
        {
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            this.trackRep.GetAll(list);
            return list;
        }
        #endregion

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