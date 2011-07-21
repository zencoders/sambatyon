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

namespace TransportService.Messages
{
    /// <summary>
    /// Class representing the message used to return the payload to a requestor.
    /// </summary>
    [DataContract]
    public class ChunkResponse : GenericMessage
    {
        private int servingBuffer;
        private byte[] payload;

        /// <summary>
        /// Default constructor of the class
        /// </summary>
        public ChunkResponse() { }

        /// <summary>
        /// Constructor of the class that initialize attributes of the message.
        /// </summary>
        /// <param name="servingBuffer">Serving buffer of te peer (that send the message). It is used for load balance</param>
        /// <param name="RID">Resource identificator</param>
        /// <param name="CID">Chunk identificator (foundamentally a progressive integer)</param>
        /// <param name="payload">Payload of the chunk</param>
        /// <param name="SenderAddress">Address of the sender of this message</param>
        public ChunkResponse(int servingBuffer, string RID, int CID, byte[] payload, Uri SenderAddress)
        {
            this.MessageType = "CHKRS";
            this.servingBuffer = servingBuffer;
            this.RID = RID;
            this.CID = CID;
            this.payload = payload;
            this.SenderAddress = SenderAddress;
        }

        #region Properties
        /// <summary>
        /// Property used to work on ServingBuffer value
        /// </summary>
        [DataMember]
        public int ServingBuffer
        {
            get { return servingBuffer; }
            set { servingBuffer = value; }
        }

        /// <summary>
        /// Property used to work on payload.
        /// </summary>
        [DataMember]
        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }
        #endregion
    }
}
