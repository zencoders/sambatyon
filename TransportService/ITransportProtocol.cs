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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TransportService.Messages;

namespace TransportService
{
    /// <summary>
    /// Interface representing the contract of a TransportService.
    /// The transport service is used to move chunks from a node to another in the network and then, it
    /// exposes fundamentally two oneway methods, the first to request a chunk to a peer and the second
    /// to return chunks to requestors.
    /// </summary>
    [ServiceContract]
    public interface ITransportProtocol
    {
        /// <summary>
        /// This method is used to require a chunk to a peer. The method is a oneway method because is
        /// supposed to be in use a Udp Binding.
        /// </summary>
        /// <param name="chkrq">
        /// Message passed to the method over the network. It contains details
        /// about the sender of the message and also to the chunk requested.
        /// </param>
        /// <seealso cref="TransportService.Messages.ChunkRequest"/>
        /// <seealso cref="UdpTransportBinding.NetUdpBinding"/>
        [OperationContract(IsOneWay=true)]
        void GetChunk(ChunkRequest chkrq);

        /// <summary>
        /// This method is used to return a chunk to a requestor peer. The method is a oneway method because
        /// it is supposed to be in use a Udp Binding.
        /// </summary>
        /// <param name="chkrs">
        /// Message passed to the method over the network. It contains the chunk
        /// to send to the requestor and information about the traffic.
        /// </param>
        /// <seealso cref="TransportService.Messages.ChunkResponse"/>
        [OperationContract(IsOneWay = true)]
        void ReturnChunk(ChunkResponse chkrs);
    }
}