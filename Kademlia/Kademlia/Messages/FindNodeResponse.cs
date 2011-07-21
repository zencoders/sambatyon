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
	/// A response to a FindNode message.
	/// Contains a list of Contacts.
	/// </summary>
	[DataContract]
	public class FindNodeResponse : Response
	{
		private List<Contact> contacts;
		
        /// <summary>
        /// Constructor of the class. It calls the base constructor and store contacts into the message.
        /// </summary>
        /// <param name="nodeID">Identificator of the sender</param>
        /// <param name="request">FindNode request that orginates this response</param>
        /// <param name="recommended">List of contact that could match the request</param>
        /// <param name="nodeEndpoint">Address of the sender</param>
		public FindNodeResponse(ID nodeID, FindNode request, List<Contact> recommended, Uri nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			contacts = recommended;
		}
		
		/// <summary>
		/// Gets the list of recommended contacts.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public List<Contact> Contacts
		{
            get {return contacts;}
            set { this.contacts = value; }
		}
		
        /// <summary>
        /// Name of the message over the network
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "FIND_NODE_RESPONSE"; }
            set { }
		}
	}
}
