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
	/// A message used to search for a node.
	/// </summary>
	[DataContract]
	public class FindNode : Message
	{
		private ID target;
		
		/// <summary>
		/// Make a new FIND_NODE message
		/// </summary>
		/// <param name="nodeID">the identificator of the sender</param>
		/// <param name="toFind">The ID of the node searched</param>
        /// <param name="nodeEndpoint">The address of the sender</param>
		public FindNode(ID nodeID, ID toFind, Uri nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			target = toFind;
		}
		
		/// <summary>
		/// Get/Set the target of this message.
		/// </summary>
        [DataMember]
		public ID Target
		{
            get { return target; }
            set { this.target = value; }
		}
		
        /// <summary>
        /// Default Name of the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "FIND_NODE"; }
            set { }
		}
	}
}
