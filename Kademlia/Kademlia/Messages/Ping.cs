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
	/// Represents a ping message, used to see if a remote node is up.
	/// </summary>
	[DataContract]
	public class Ping : Message
	{
        /// <summary>
        /// Constructor of the ping message class
        /// </summary>
        /// <param name="senderID">Identificator of te sender</param>
        /// <param name="nodeEndpoint">Endpoint of the sender</param>
		public Ping(ID senderID, Uri nodeEndpoint) : base(senderID, nodeEndpoint)
		{
		}
		
        /// <summary>
        /// Default Property indicating the name of the message.
        /// </summary>
        [DataMember]
		public override string Name
		{
            get { return "PING"; }
            set { }
		}
	}
}
