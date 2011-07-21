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
	/// A reply to a store query.
	/// </summary>
	[DataContract]
	public class StoreResponse : Response
	{
		private bool sendData;
		
        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="nodeID">Identificator of the sender</param>
        /// <param name="query">StoreQuery originating this message</param>
        /// <param name="accept">Param that indicates if the node has accepted the request to store</param>
        /// <param name="nodeEndpoint">Address of the sender node</param>
		public StoreResponse(ID nodeID, StoreQuery query, bool accept, Uri nodeEndpoint) : base(nodeID, query, nodeEndpoint)
		{
			sendData = accept;
		}
		
		/// <summary>
		/// Indicator to verify if it is necessary or not to send data
		/// </summary>
        [DataMember]
		public bool ShouldSendData
		{
            get { return sendData; }
            set { this.sendData = value; }
		}
		
        /// <summary>
        /// Default name of the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "STORE_RESPONSE"; }
            set { }
		}
	}
}
