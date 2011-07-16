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
using Persistence.Tag;

namespace Kademlia.Messages
{
	/// <summary>
	/// Send along the data in response to an affirmative StoreResponse.
	/// </summary>
	[DataContract]
	public class StoreData : Response
	{
		private CompleteTag data;
		private DateTime publication;
        private Uri transportUri;
		
		/// <summary>
		/// Make a mesage to store the given data.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="theKey"></param>
		/// <param name="theDataHash"></param>
		/// <param name="theData"></param>
		/// <param name="originalPublication"></param>
		public StoreData(ID nodeID, StoreResponse request, CompleteTag theData, DateTime originalPublication, Uri nodeEndpoint, Uri transportUri) : base(nodeID, request, nodeEndpoint)
		{
			this.data = theData;
			this.publication = originalPublication;
            this.transportUri = transportUri;
		}
		
		/// <summary>
		/// Get the data to store.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public CompleteTag Data
		{
            get { return data; }
            set { this.data = value; }
		}
		
		/// <summary>
		/// Get when the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public DateTime PublicationTime
		{
            get { return publication.ToUniversalTime(); }
            set { this.publication = value; }
		}

        [DataMember]
        public Uri TransportUri
        {
            get { return transportUri; }
            set { this.transportUri = value; }
        }

        [DataMember]
		public override string Name
		{
            get { return "STORE_DATA"; }
            set { }
		}
	}
}
