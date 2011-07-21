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
using Persistence;

namespace Kademlia.Messages
{
	/// <summary>
	/// Send data in reply to a FindVAlue
	/// </summary>
	[DataContract]
	public class FindValueDataResponse : Response
	{
		private IList<KademliaResource> vals;
		
		/// <summary>
		/// Make a new response.
		/// </summary>
		/// <param name="nodeID">The identificator of the sender</param>
		/// <param name="request">The FindValue request generating this response</param>
		/// <param name="data">The list of KademliaResources found</param>
        /// <param name="nodeEndpoint">The address of the sender</param>
		public FindValueDataResponse(ID nodeID, FindValue request, IList<KademliaResource> data, Uri nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			vals = data;
		}
		
		/// <summary>
		/// the values returned for the key
		/// </summary>
        [DataMember]
		public IList<KademliaResource> Values
		{
            get { return vals; }
            set { this.vals = value; }
		}
		
        /// <summary>
        /// The default name of the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE_RESPONSE_DATA"; }
            set { }
		}
	}
}
