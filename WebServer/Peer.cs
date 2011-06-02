using System;
using System.Collections.Generic;
using System.ServiceModel;
using TransportService;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using Kademlia;

namespace PeerPlayer
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    class Peer : IDisposable, IPeer
    {
        private TransportProtocol transportLayer;
        private Dht kademliaLayer;
        private Stream localStream;
        private ServiceHost[] svcHosts = new ServiceHost[3];

        private ServiceHost RunInterfaceLayer()
        {
            Console.WriteLine("Running Interface Layer.....");
            ServiceHost host = new ServiceHost(this);
            host.Open();
            foreach (Uri uri in host.BaseAddresses)
            {
                Console.WriteLine("\t{0}", uri.ToString());
            }
            return host;
        }

        private ServiceHost RunTransportLayer()
        {
            string udpPort = PeerPlayer.Properties.Settings.Default.udpPort;
            Uri[] addresses = new Uri[1];
            addresses[0] = new Uri("soap.udp://localhost:" + udpPort + "/TransportProtocol/");
            TransportProtocol tsp = new TransportProtocol(addresses[0]);
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
                Console.WriteLine("Number of dispatchers listening : {0}", host.ChannelDispatchers.Count);
                foreach (System.ServiceModel.Dispatcher.ChannelDispatcher dispatcher in host.ChannelDispatchers)
                {
                    Console.WriteLine("\t{0}", dispatcher.Listener.Uri.ToString());
                }
                #endregion
            }
            catch (AddressAlreadyInUseException)
            {
                Console.WriteLine("Unable to Connect as a Server because there is already one on this machine");
            }
            return host;
        }

        private ServiceHost RunKademliaLayer(bool single, string btpNode)
        {
            string kademliaPort = PeerPlayer.Properties.Settings.Default.kademliaPort;
            KademliaNode node = new KademliaNode(new EndpointAddress("soap.udp://localhost:"+kademliaPort+"/kademlia"));
            this.kademliaLayer = new Dht(PeerPlayer.Properties.Settings.Default.nodes, node, single, btpNode);
            ServiceHost kadHost = new ServiceHost(node, new Uri("soap.udp://localhost:"+kademliaPort+"/kademlia"));
            kadHost.Open();
            return kadHost;
        }

        public Peer(bool single = false, string btpNode = "")
        {
            this.localStream = new MemoryStream();
         //   svcHosts[0] = this.RunKademliaLayer(single, btpNode);
            svcHosts[2] = this.RunTransportLayer();
            svcHosts[1] = this.RunInterfaceLayer();
        }

        public void Configure(string httpPort = "-1", string tcpPort = "-1", string udpPort = "-1", string kademliaPort = "-1", string dbFile = "")
        {
            if (httpPort != "-1")
            {
                PeerPlayer.Properties.Settings.Default.httpPort = httpPort;
            }
            if (tcpPort != "-1")
            {
                PeerPlayer.Properties.Settings.Default.tcpPort = tcpPort;
            }
            if (udpPort != "-1")
            {
                PeerPlayer.Properties.Settings.Default.udpPort = udpPort;
            }
            if (kademliaPort != "-1")
            {
                PeerPlayer.Properties.Settings.Default.kademliaPort = kademliaPort;
            }
            if (dbFile != "")
            {
                PeerPlayer.Properties.Settings.Default.dbFile = dbFile;
            }
            PeerPlayer.Properties.Settings.Default.Save();
        }

        public Stream ConnectToStream()
        {
            return this.localStream;
        }

        public void GetFlow(string RID, int begin, int length, Dictionary<string, float> nodes)
        {
            this.transportLayer.start(RID, begin, length, nodes, this.localStream);
        }

        public void StopFlow()
        {
            this.transportLayer.stop();
        }

        static void Main(string[] args)
        {
            using (Peer p = new Peer())
            {
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
        }
    }
}