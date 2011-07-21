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
    /// <summary>
    /// Interface class for Kademlia. In our implementation, Kademlia is a WCF Service.
    /// This interface represents the service contract of Kademlia.
    /// All methods in this interface are oneway.
    /// </summary>
    [ServiceContract]
    public interface IKademliaNode
    {
        /// <summary>
        /// This is the generic method used to handle a message over the network. In its simply
        /// implementation it has to cache info about the sender of the message.
        /// </summary>
        /// <param name="msg">A Message instance</param>
        [OperationContract(IsOneWay = true)]
        void HandleMessage(Message msg);

        /// <summary>
        /// This method is used to handle a response. It has to store informations in a cache about
        /// the sender.
        /// </summary>
        /// <param name="response">A Response instance</param>
        [OperationContract(IsOneWay = true)]
        void CacheResponse(Response response);

        /// <summary>
        /// This method is used to receive a Ping message. In general, ping messages are used in
        /// bootstrap phase of the node to check another node is up.
        /// </summary>
        /// <param name="ping">A Ping message object</param>
        [OperationContract(IsOneWay = true)]
        void HandlePing(Ping ping);

        /// <summary>
        /// This method is used to receive a Pong message. A pong is the response to a Ping message
        /// that signal that the node is alive.
        /// </summary>
        /// <param name="pong">A pong message object</param>
        [OperationContract(IsOneWay = true)]
        void HandlePong(Pong pong);

        /// <summary>
        /// This method is used to handle a FindNode request. This method has to find all nodes matching
        /// request's parameters and to send a FindNodeResponse.
        /// </summary>
        /// <param name="request">A FindNode message</param>
        [OperationContract(IsOneWay = true)]
        void HandleFindNode(FindNode request);

        /// <summary>
        /// This method is used to handle a FindNodeResponse. This method handles the FindNodeResponse and
        /// caches the results.
        /// </summary>
        /// <param name="response">A FindNodeResponse message</param>
        [OperationContract(IsOneWay = true)]
        void HandleFindNodeResponse(FindNodeResponse response);

        /// <summary>
        /// This method is used to handle a FindValue request. When a node receive a request like that
        /// is has two possibilities; if it has the value searched it reply with a FindValueResponse instead
        /// it sends to the requestor a FindValueContactResponse.
        /// </summary>
        /// <param name="request">A FindValue request message</param>
        [OperationContract(IsOneWay = true)]
        void HandleFindValue(FindValue request);

        /// <summary>
        /// This method is used to handle a FindValueDataResponse. When the message is received, it is
        /// returned as a list to the user.
        /// </summary>
        /// <param name="response">A FindValueDataResponse message</param>
        [OperationContract(IsOneWay = true)]
        void HandleFindValueDataResponse(FindValueDataResponse response);

        /// <summary>
        /// Method used to handle a FindValueContactResponse. When the message is received, the contact
        /// stored inside are used to require another FindValue.
        /// </summary>
        /// <param name="response">A FindValueContactResponse message</param>
        [OperationContract(IsOneWay = true)]
        void HandleFindValueContactResponse(FindValueContactResponse response);

        /// <summary>
        /// Method used to handle a StoreQuery request. When the message is received, the peer evaluates
        /// if the skew of the request is acceptable and that the request is not too old and, if so, the 
        /// method sends back to the requestor a HandleStoreResponse that announce the possibility to
        /// store the resource.
        /// </summary>
        /// <param name="request">A StoreQuery message</param>
        [OperationContract(IsOneWay = true)]
        void HandleStoreQuery(StoreQuery request);

        /// <summary>
        /// Method used to receive a StoreResponse message. When this message is received a StoreData is
        /// immediatly sent to the requestor.
        /// </summary>
        /// <param name="response">A StoreResponse message</param>
        [OperationContract(IsOneWay = true)]
        void HandleStoreResponse(StoreResponse response);

        /// <summary>
        /// Method used to handle a StoreData message. The message passed actively contains the data to store
        /// </summary>
        /// <param name="request">A StoreData message containing data to store.</param>
        [OperationContract(IsOneWay = true)]
        void HandleStoreData(StoreData request);
    }
}
