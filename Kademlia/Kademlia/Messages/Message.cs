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
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Represents a generic DHT RPC message
	/// </summary>
	[DataContract]
	public abstract class Message
	{
		// All messages include sender id and address
		private ID senderID;
        private Uri nodeEndpoint;
		private ID conversationID;
		
		/// <summary>
		/// Make a new message, recording the sender's ID.
		/// </summary>
		/// <param name="senderID">The sender identificator</param>
        /// <param name="nodeEndpoint">The sender endpoint</param>
		public Message(ID senderID, Uri nodeEndpoint) {
			this.senderID = senderID;
			this.conversationID = ID.RandomID();
            this.nodeEndpoint = nodeEndpoint;
		}
		
		/// <summary>
		/// Make a new message in a given conversation.
		/// </summary>
		/// <param name="senderID">The sender identificator</param>
		/// <param name="conversationID">The conversationID regarding the message</param>
        /// <param name="nodeEndpoint">the address of the sender</param>
		public Message(ID senderID, ID conversationID, Uri nodeEndpoint) {
			this.senderID = senderID;
			this.conversationID = conversationID;
            this.nodeEndpoint = nodeEndpoint;
		}
		
		/// <summary>
		/// the name of the message.
		/// </summary>
        [DataMember]
        public abstract string Name
        {
            get;
            set;
        }
		
		/// <summary>
		/// the ID of the sender of the message.
		/// </summary>
        [DataMember]
		public ID SenderID
        {
            get { return senderID; }
            set { this.senderID = value; }
		}
		
		/// <summary>
		/// the ID of this conversation.
		/// </summary>
        [DataMember]
		public ID ConversationID
        {
            get { return conversationID; }
            set { this.conversationID = value; }
		}

        /// <summary>
        /// the address of the sender
        /// </summary>
        [DataMember]
        public Uri NodeEndpoint
        {
            get { return nodeEndpoint; }
            set { this.nodeEndpoint = value; }
        }
	}
}
