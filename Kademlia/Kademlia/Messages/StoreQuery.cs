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
	/// A message asking if another node will store data for us, and if we need to send the data.
	/// Maybe they already have it.
	/// We have to send the timestamp with it for people to republish stuff.
	/// </summary>
	[DataContract]
	public class StoreQuery : Message
	{
		private ID tagHash;
		private DateTime publication;
		
		/// <summary>
		/// Make a new STORE_QUERY message.
		/// </summary>
		/// <param name="nodeID">The identificator of the sender</param>
		/// <param name="hash">A hash of the data value</param>
		/// <param name="originalPublication">The time of publication</param>
        /// <param name="nodeEndpoint">The address of the sender</param>
		public StoreQuery(ID nodeID, ID hash, DateTime originalPublication, Uri nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			tagHash = hash;
			publication = originalPublication;
		}
		
		/// <summary>
		/// The hash of the data value we're asking about.
		/// </summary>
        [DataMember]
		public ID TagHash
		{
            get { return tagHash; }
            set { this.tagHash = value; }
		}

		/// <summary>
		/// the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public DateTime PublicationTime
		{
            get { return publication.ToUniversalTime(); }
            set { this.publication = value; }
		}
		
        /// <summary>
        /// The default name of the message
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "STORE_QUERY"; }
            set { }
		}
	}
}
