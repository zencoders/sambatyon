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
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using Persistence;

namespace PeerLibrary
{
    /// <summary>
    /// Interface of the peer. It has been implemented using a WCF interface cause to the future intention
    /// to implement a remote GUI for the peer that invokes methods of the peer as contract functions.
    /// </summary>
    [ServiceContract]
    interface IPeer
    {
        /// <summary>
        /// Method used to connect to a previously initialized stream
        /// </summary>
        /// <returns>The stream to use</returns>
        [OperationContract]
        Stream ConnectToStream();

        /// <summary>
        /// Method used to restart a flow download. It uses the method to recover stream download without
        /// reinitialization.
        /// </summary>
        [OperationContract]
        void RestartFlow();

        /// <summary>
        /// Method used to get a file flow from the network.
        /// </summary>
        /// <param name="RID">The resource identifier</param>
        /// <param name="begin">The begin point from the head of a file to start download</param>
        /// <param name="length">The total length of file</param>
        /// <param name="nodes">Nodes with associated score that are used to download a file from the network</param>
        [OperationContract]
        void GetFlow(string RID, int begin, long length, Dictionary<string, float> nodes);

        /// <summary>
        /// Method used to stop the flow.
        /// </summary>
        [OperationContract]
        void StopFlow();

        /// <summary>
        /// Method used to reconfigure peer settings (protocols ports, ecc...)
        /// </summary>
        /// <param name="udpPort">String containing the value of the udpPort</param>
        /// <param name="kademliaPort">String containig the value of the kademliaPort</param>
        [OperationContract]
        void Configure(string udpPort, string kademliaPort);

        /// <summary>
        /// Method used to store a file into the peer datastore repository. It is used to allow new files
        /// to be downloadable using the system
        /// </summary>
        /// <param name="filename">filename of the file to download</param>
        /// <returns>true if the filename have been store; false otherwise</returns>
        [OperationContract]
        bool StoreFile(string filename);

        /// <summary>
        /// Method used to search a file by a queryString
        /// </summary>
        /// <param name="queryString">the querystring used to search</param>
        /// <returns>A list of found resources</returns>
        [OperationContract]
        IList<KademliaResource> SearchFor(string queryString);
    }
}
