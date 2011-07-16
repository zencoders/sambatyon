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
using Kademlia.Messages;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia
{
    [ServiceContract]
    public interface IKademliaNode
    {
        [OperationContract(IsOneWay = true)]
        void HandleMessage(Message msg);

        [OperationContract(IsOneWay = true)]
        void CacheResponse(Response response);

        [OperationContract(IsOneWay = true)]
        void HandlePing(Ping ping);

        [OperationContract(IsOneWay = true)]
        void HandlePong(Pong pong);

        [OperationContract(IsOneWay = true)]
        void HandleFindNode(FindNode request);

        [OperationContract(IsOneWay = true)]
        void HandleFindNodeResponse(FindNodeResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleFindValue(FindValue request);

        [OperationContract(IsOneWay = true)]
        void HandleFindValueDataResponse(FindValueDataResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleFindValueContactResponse(FindValueContactResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleStoreQuery(StoreQuery request);

        [OperationContract(IsOneWay = true)]
        void HandleStoreResponse(StoreResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleStoreData(StoreData request);
    }
}
