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

namespace PeerPlayer
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    class Peer : IDisposable, IPeer
    {
        private TransportProtocol transportLayer;
        private Dht kademliaLayer;
        private Stream localStream;
        private ServiceHost[] svcHosts = new ServiceHost[3];
        private bool single;
        private string btpNode;
        private Persistence.Repository trackRep;

        public Dictionary<string, string> ConfOptions {get; set;}

        public Peer(bool single = false, string btpNode = "")
        {
            this.ConfOptions = new Dictionary<string, string>();
            this.ConfOptions["udpPort"] = PeerPlayer.Properties.Settings.Default.udpPort;
            this.ConfOptions["kadPort"] = PeerPlayer.Properties.Settings.Default.kademliaPort;
            this.localStream = new MemoryStream();
            this.single = single;
            this.btpNode = btpNode;
            AppSettingsReader asr = new AppSettingsReader();
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(new { data_dir = (string)asr.GetValue("TrackRepository", typeof(string)) });
            this.trackRep = Persistence.RepositoryFactory.GetRepositoryInstance("Raven", conf);
        }

        public void runLayers(bool withoutInterface=false)
        {
            svcHosts[0] = this.runKademliaLayer(single, btpNode);
            svcHosts[1] = this.runTransportLayer();
            if(!withoutInterface)
                svcHosts[2] = this.runInterfaceLayer();
        }

        #region layersInitialization
        private ServiceHost runInterfaceLayer()
        {
            Console.WriteLine("Running Interface Layer.....");
            ServiceHost host = new ServiceHost(this);
            try
            {
                host.Open();
            }
            catch (AddressAlreadyInUseException)
            {
                Console.WriteLine("Unable to Connect as a Server because there is already one on this machine");
            }
            foreach (Uri uri in host.BaseAddresses)
            {
                Console.WriteLine("\t{0}", uri.ToString());
            }
            return host;
        }

        private ServiceHost runTransportLayer()
        {
            Console.WriteLine("Running Transport Layer.....");
            string udpPort = this.ConfOptions["udpPort"];
            Uri[] addresses = new Uri[1];
            addresses[0] = new Uri("soap.udp://localhost:" + udpPort + "/transport_protocol/");
            TransportProtocol tsp = new TransportProtocol(addresses[0], this.trackRep);
            this.transportLayer = tsp;
            ServiceHost host = new ServiceHost(tsp, addresses);
            try
            {
                host.Open();
                #region Output dispatchers listening
                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine("\t{0}", uri.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("\tNumber of dispatchers listening : {0}", host.ChannelDispatchers.Count);
                foreach (System.ServiceModel.Dispatcher.ChannelDispatcher dispatcher in host.ChannelDispatchers)
                {
                    Console.WriteLine("\t\t{0}", dispatcher.Listener.Uri.ToString());
                }
                #endregion
            }
            catch (AddressAlreadyInUseException aaiue)
            {
                Console.WriteLine("Unable to Connect as a Server because there is already one on this machine");
                throw aaiue;
            }
            return host;
        }

        private ServiceHost runKademliaLayer(bool single, string btpNode)
        {
            string kademliaPort = this.ConfOptions["kadPort"];
            KademliaNode node = new KademliaNode(new EndpointAddress("soap.udp://localhost:"+kademliaPort+"/kademlia"));
            ServiceHost kadHost = new ServiceHost(node, new Uri("soap.udp://localhost:" + kademliaPort + "/kademlia"));
            try
            {
                kadHost.Open();
            }
            catch (AddressAlreadyInUseException aaiue)
            {
                Console.WriteLine("Unable to Connect as a Server because there is already one on this machine");
                throw aaiue;
            }
            this.kademliaLayer = new Dht(node, single, btpNode);
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            Console.WriteLine("GetAll Response : " + this.trackRep.GetAll(list));
            Parallel.ForEach(list, t =>
            {
                this.kademliaLayer.Put(t.Filename);
            });
            return kadHost;
        }
        #endregion

        #region interface

        public void Configure(string udpPort = "-1", string kademliaPort = "-1")
        {
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
            return this.localStream;
        }

        public void GetFlow(string RID, int begin, int length, Dictionary<string, float> nodes)
        {
            this.GetFlow(RID, begin, length, nodes, null);
        }
        public void GetFlow(string RID, int begin, int length, Dictionary<string, float> nodes, Stream stream = null)
        {
            Stream handlingStream = stream;
            if (handlingStream == null)
            {
                handlingStream = this.localStream;
            }
            this.transportLayer.Start(RID, begin, length, nodes, handlingStream);
        }

        public void StopFlow()
        {
            this.transportLayer.Stop();
        }

        public void StoreFile(string filename)
        {
            TrackModel track = new TrackModel(filename);
            this.trackRep.Save(track);
        }

        public IList<KademliaResource> SearchFor(string queryString)
        {
            return this.kademliaLayer.GetAll(queryString);
        }

        #endregion

        static void Main(string[] args)
        {
            bool withoutInterface = false;
            using (Peer p = new Peer())
            {
                if (args.Length % 2 != 0)
                {
                    Console.WriteLine("Error in parsing options");
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
                p.runLayers(withoutInterface);
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate Host");
                Console.ReadLine();
            }

        /*    if (args.Length < 1 || (args.Length >= 1 && args[0] != "configuration"))
            {
                string httpPort = WCFServiceHost.Properties.Settings.Default.httpPort;
                string tcpPort = WCFServiceHost.Properties.Settings.Default.tcpPort;
                string udpPort = WCFServiceHost.Properties.Settings.Default.udpPort;
                Uri[] addresses = new Uri[1];
          //      addresses[0] = new Uri("http://localhost:" + httpPort + "/TransportProtocol/");
          //      addresses[0] = new Uri("net.tcp://localhost:" + tcpPort + "/TransportProtocol/");
                addresses[0] = new Uri("soap.udp://localhost:" + udpPort + "/TransportProtocol/");
                Type serviceType = typeof(TransportProtocol);
                ServiceHost host = new ServiceHost(serviceType, addresses);
                try
                {
                    host.Open();
                    #region Output dispatchers listening
                    foreach (Uri uri in host.BaseAddresses)
                    {
                        Console.WriteLine("\t{0}", uri.ToString());
                    }
                    Console.WriteLine();
                    Console.WriteLine("Number of dispatchers listening : {0}", host.ChannelDispatchers.Count);
                    foreach (System.ServiceModel.Dispatcher.ChannelDispatcher dispatcher in host.ChannelDispatchers)
                    {
                        Console.WriteLine("\t{0}, {1}", dispatcher.Listener.Uri.ToString(), dispatcher.BindingName);
                    }

                    Console.WriteLine();
                    Console.WriteLine("Press <ENTER> to terminate Host");
                    Console.ReadLine();
                    #endregion
                }
                catch (AddressAlreadyInUseException)
                {
                    Console.WriteLine("Unable to Connect as a Server because there is already one on this machine");
                }
            }
            //Example CODE
            if(args.Length >= 1 && args[0] != "configuration") {
//                ChunkRequest chkrq = new ChunkRequest(System.Convert.ToInt32(args[0]), args[1], System.Convert.ToInt32(args[2]));
/*                Transferer tsf = new Transferer();
                Dictionary<string, float> p2p = new Dictionary<string,float>();
                p2p["net.tcp://localhost:9999/TransportProtocol"] = (float)0.50;
                System.Console.WriteLine("AAAAAAAAAAAAA");
                System.Console.WriteLine(p2p.Count());
                MemoryStream s = new MemoryStream();
                tsf.start("AIAIA", 2, 1000, p2p, s);
                StreamReader sr = new StreamReader(s);
                while (true)
                {
                    while (sr.Peek() >= 0) 
                    {
                        Console.Write("AAA" + (char)sr.Read());
                    }
                }
//                Console.WriteLine("Remote Serving Buffer is: {0}", result.ServingBuffer);
            }
            else if (args.Length >= 1 && args[0] == "configuration")
            {
                Console.WriteLine("New Http Port:");
                WCFServiceHost.Properties.Settings.Default.httpPort = Console.ReadLine();
                Console.WriteLine("New Tcp Port:");
                WCFServiceHost.Properties.Settings.Default.tcpPort = Console.ReadLine();
                WCFServiceHost.Properties.Settings.Default.Save();
            }*/
        }

        public void Dispose()
        {
            foreach (ServiceHost svcHost in this.svcHosts)
            {
                if(svcHost != null)
                    svcHost.Close();
            }
            this.trackRep.Dispose();
        }
    }
}