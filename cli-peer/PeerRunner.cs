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
