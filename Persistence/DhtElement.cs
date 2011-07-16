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
    [Serializable]
    [DataContractAttribute]
    public class DhtElement: IEquatable<DhtElement>,IEquatable<Uri>/*, ISerializable*/
    {
        #region Properties
        [DataMemberAttribute]
        public Uri Url
        {
            get;
            set;
        }
        [DataMemberAttribute]
        public DateTime Publication
        {
            get;
            set;
        }
        [DataMemberAttribute]
        public TimeSpan Validity
        {
            get;
            set;
        }
        #endregion
        public DhtElement()
        {
        }
        public DhtElement(Uri u, DateTime p, TimeSpan v) :
            this()
        {
            this.Url = u;
            this.Publication = p;
            this.Validity = v;
        }

        public DhtElement(SerializationInfo info, StreamingContext ctxt)
            : this()
        {
            this.Url = (Uri) info.GetValue("url", typeof(Uri));
            this.Publication = (DateTime) info.GetValue("publication", typeof(DateTime));
            this.Validity = (TimeSpan) info.GetValue("validity", typeof(TimeSpan));
        }


        #region IEquatable<DhtElement>

        public bool Equals(DhtElement other)
        {
            return other.Url.Equals(this.Url);
        }

        #endregion

        #region IEquatable<Uri>

        public bool Equals(Uri other)
        {
            return other.Equals(this.Url);
        }

        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("url", this.Url);
            info.AddValue("publication", this.Publication);
            info.AddValue("validity", this.Validity);
        }


        public override int GetHashCode()
        {
            return this.Url.ToString().GetHashCode();
        }
    }
}
