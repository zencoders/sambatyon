using System;
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

namespace PeerPlayer
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
            this.ConfOptions["udpPort"] = PeerPlayer.Properties.Settings.Default.udpPort;
            this.ConfOptions["kadPort"] = PeerPlayer.Properties.Settings.Default.kademliaPort;
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
            Thread kadThread = new Thread(new ThreadStart(()=>this.runKademliaLayer(single, btpNode, ref svcHosts[0])));
            Thread transportThread = new Thread(new ThreadStart(()=>this.runTransportLayer(ref svcHosts[1])));
            if (!withoutInterface)
            {
                Thread interfaceThread = new Thread(new ThreadStart(() => this.runInterfaceLayer(ref svcHosts[2])));
                interfaceThread.Start();
            }
            kadThread.Start();
            transportThread.Start();
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
            this.kademliaLayer = new Dht(node, single, btpNode);
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
                PeerPlayer.Properties.Settings.Default.udpPort = udpPort;
            }
            if (kademliaPort != "-1")
            {
                PeerPlayer.Properties.Settings.Default.kademliaPort = kademliaPort;
            }
            PeerPlayer.Properties.Settings.Default.Save();
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

        static void Main(string[] args)
        {
            bool withoutInterface = false;
            using (Peer p = new Peer())
            {
                if (args.Length % 2 != 0)
                {
                    log.Error("Error in parsing options");
                    return;
                }
                else
                {
                    bool storeConf = false;
                    for (int i = 0; i < args.Length; i += 2)
                    {
                        if (args[i] == "--udpPort" || args[i] == "-u")
                        {
                            p.ConfOptions["udpPort"] = args[i + 1];
                        }
                        else if (args[i] == "--kadPort" || args[i] == "-k")
                        {
                            p.ConfOptions["kadPort"] = args[i + 1];
                        }
                        else if ((args[i] == "--store" || args[i] == "-s") && (args[i + 1] == "1"))
                        {
                            storeConf = true;
                        }
                        else if ((args[i] == "--without_interface" || args[i] == "-i") && (args[i + 1] == "1"))
                        {
                            withoutInterface = true;
                        }
                    }
                    if (storeConf)
                    {
                        p.Configure(p.ConfOptions["udpPort"], p.ConfOptions["kadPort"]);
                    }
                }
                p.RunLayers(withoutInterface);
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate Host");
                Console.ReadLine();
            }
        }

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