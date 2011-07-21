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
	/// Description of FindKeyContactResponse.
	/// </summary>
	[DataContract]
	public class FindValueContactResponse : Response
	{
		private List<Contact> contacts;
		
		/// <summary>
		/// Make a new response reporting contacts to try.
		/// </summary>
		/// <param name="nodeID">the sender identificator</param>
		/// <param name="request">The FindValue message orginating this response</param>
		/// <param name="close">The list of suggested contacts</param>
        /// <param name="nodeEndpoint">The address of the sender</param>
		public FindValueContactResponse(ID nodeID, FindValue request, List<Contact> close, Uri nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			contacts = close;
		}
		
		/// <summary>
		/// The list of contacts sent.
		/// </summary>
        [DataMember]
		public List<Contact> Contacts
		{
            get { return contacts; }
            set { this.contacts = value; }
		}
		
        /// <summary>
        /// The default name for the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE_RESPONSE_CONTACTS"; }
            set { }
		}
	}
}
