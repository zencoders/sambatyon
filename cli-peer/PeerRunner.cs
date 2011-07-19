using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeerLibrary;
//using log4net;

namespace cli_peer
{
    class PeerRunner
    {
    //    private static readonly ILog log = LogManager.GetLogger(typeof(PeerRunner));
        static void Main(string[] args)
        {
            bool withoutInterface = true;
            using (Peer p = new Peer())
            {
                if (args.Length % 2 != 0)
                {
      //              log.Error("Error in parsing options");
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
                        else if ((args[i] == "--with_interface" || args[i] == "-i") && (args[i + 1] == "1"))
                        {
                            withoutInterface = false;
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
    }
}
