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
using System.Net;
using System.ServiceModel;

namespace Kademlia
{
	/// <summary>
	/// Represents the information needed to contact another node.
	/// </summary>
	[Serializable]
	public class Contact
	{
		private ID nodeID;
        private Uri nodeEndpoint;
		
		/// <summary>
		/// Make a contact for a node with the given ID at the given location.
		/// </summary>
		/// <param name="id">The identificator of the node</param>
		/// <param name="endpoint">The address of the node</param>
		public Contact(ID id, Uri endpoint)
		{
			nodeID = id;
			nodeEndpoint = endpoint;
		}
		
		/// <summary>
		/// Get the node's ID.
		/// </summary>
		/// <returns>The node ID</returns>
		public ID NodeID {
            get { return nodeID; }
		}
		
		/// <summary>
		/// Get the node's endpoint.
		/// </summary>
		/// <returns>The address</returns>
		public Uri NodeEndPoint {
            get { return nodeEndpoint; }
		}
		
        /// <summary>
        /// Method used to obtain a string representation of the class
        /// </summary>
        /// <returns>A string representing the object</returns>
		public override string ToString()
		{
			return NodeID.ToString() + "@" + NodeEndPoint.ToString();
		}
	}
}
