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
	/// Represents a request to get a value.
	/// Receiver should either send key or a node list.
	/// </summary>
	[DataContract]
	public class FindValue : Message
	{
		private string key;
		
		/// <summary>
		/// Make a new FindValue message.
		/// </summary>
		/// <param name="nodeID">The sender identificator</param>
		/// <param name="wantedKey">The desired key by the sender</param>
        /// <param name="nodeEndpoint">The address of the sender</param>
		public FindValue(ID nodeID, string wantedKey, Uri nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			this.key = wantedKey;
		}
		
		/// <summary>
		/// The key that the message searches.
		/// </summary>
        [DataMember]
		public string Key {
            get { return key; }
            set { this.key = value; }
		}
		
        /// <summary>
        /// the default name of the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE"; }
            set { }
		}
	}
}
