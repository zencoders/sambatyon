using System;
using System.Collections.Generic;
using System.ServiceModel;
using TransportService;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Type serviceType = typeof(TransportProtocol);
                ServiceHost host = new ServiceHost(serviceType);

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
                Console.WriteLine("Unable to Connect like a Server because there is already one on this machine");
            }
            //Example CODE
            if(args.Length >= 1) {
//                ChunkRequest chkrq = new ChunkRequest(System.Convert.ToInt32(args[0]), args[1], System.Convert.ToInt32(args[2]));
                Transferer tsf = new Transferer();
                Dictionary<string, float> p2p = new Dictionary<string,float>();
                p2p["localhost:9999"] = (float)0.50;
                System.Console.WriteLine("AAAAAAAAAAAAA");
                System.Console.WriteLine(p2p.Count());
                MemoryStream s = new MemoryStream();
                tsf.start("AIAIA", 2, 1000, p2p, s);
                StreamReader sr = new StreamReader(s);
                while (true)
                {
                    System.Console.Write(sr.Read());
                }
//                Console.WriteLine("Remote Serving Buffer is: {0}", result.ServingBuffer);
            }
            //
        }
    }
}

