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

namespace Persistence
{
    /// <summary>
    /// This class represent an element inside the Kademlia DHT (Distributed Hash Table).
    /// Each element has a URL, a pubblication time and a validity period (after which the element will expire)
    /// Generaly objects of this class are contained into a Kademlia Resource
    /// </summary>
    [Serializable]
    [DataContractAttribute]
    public class DhtElement: IEquatable<DhtElement>,IEquatable<Uri>/*, ISerializable*/
    {
        #region Properties
        /// <summary>
        /// URL of a node in the DHT
        /// </summary>
        [DataMemberAttribute]
        public Uri Url
        {
            get;
            set;
        }
        /// <summary>
        /// Publication time of the element
        /// </summary>
        [DataMemberAttribute]
        public DateTime Publication
        {
            get;
            set;
        }
        /// <summary>
        /// Validity period of the element
        /// </summary>
        [DataMemberAttribute]
        public TimeSpan Validity
        {
            get;
            set;
        }
        #endregion
        /// <summary>
        /// Default constructor of the Element. This does nothing
        /// </summary>
        public DhtElement()
        {
        }
        /// <summary>
        /// Main constructor of the Element. This just initializes all the fields with the given value.
        /// </summary>
        /// <param name="u">URL of the element</param>
        /// <param name="p">Publiation Time of the Element</param>
        /// <param name="v">Validity Period of the Element</param>
        public DhtElement(Uri u, DateTime p, TimeSpan v) :
            this()
        {
            this.Url = u;
            this.Publication = p;
            this.Validity = v;
        }
        /// <summary>
        /// Method used for serialization purpose
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public DhtElement(SerializationInfo info, StreamingContext ctxt)
            : this()
        {
            this.Url = (Uri) info.GetValue("url", typeof(Uri));
            this.Publication = (DateTime) info.GetValue("publication", typeof(DateTime));
            this.Validity = (TimeSpan) info.GetValue("validity", typeof(TimeSpan));
        }


        #region IEquatable<DhtElement>

        /// <summary>
        /// Method that enable equality comparison between DhtElement objects.
        /// The comparison is based on URL <c>Equals</c> method.
        /// </summary>
        /// <param name="other">Object to compare</param>
        /// <returns>True if the Element are equals, false otherwise</returns>
        public bool Equals(DhtElement other)
        {
            return other.Url.Equals(this.Url);
        }

        #endregion

        #region IEquatable<Uri>


        /// <summary>
        /// Method that enable equality comparison between DhtElement and Uri Object.
        /// THe comparison is based on URL <c>Equals</c> method.
        /// </summary>
        /// <param name="other">Url to compare</param>
        /// <returns>True if the Element's Url and the Uri passed is equals, false otherwise</returns>
        public bool Equals(Uri other)
        {
            return other.Equals(this.Url);
        }

        #endregion


        /// <summary>
        /// Method used for serialization purpose
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("url", this.Url);
            info.AddValue("publication", this.Publication);
            info.AddValue("validity", this.Validity);
        }

        /// <summary>
        /// Returns Element HashCode to use in Hashset
        /// This relies on HashCode of the string representation of the URL
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Url.ToString().GetHashCode();
        }
    }
}
