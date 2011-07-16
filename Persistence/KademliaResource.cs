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
using Persistence.Tag;
using System.Runtime.Serialization;

namespace Persistence
{
    [Serializable]
    [DataContractAttribute]
    public class KademliaResource: IDocumentType,ILoadable/*,ISerializable*/
    {
        public KademliaResource()
        {
            this.Tag = null;
            this.Urls = new HashSet<DhtElement>();

        }
        public KademliaResource(string filename, params DhtElement[] urls) :this() {
            this.Tag = new CompleteTag(filename);
            if (urls.Length != 0)
            {
                foreach(DhtElement e in urls)
                {
                    this.Urls.Add(e);
                }
//                this.Urls.Union<DhtElement>(urls, new DhtElementComparer());
            }
        }
        public KademliaResource(CompleteTag tag, params DhtElement[] urls) : this()
        {
            this.Tag = tag;
            if (urls.Length != 0)
            {
                foreach (DhtElement e in urls)
                {
                    this.Urls.Add(e);
                }
//                this.Urls.Union<DhtElement>(urls, new DhtElementComparer());
            }
        }
        public KademliaResource(SerializationInfo info, StreamingContext ctxt) : this()
        {
            this.Tag = (CompleteTag)info.GetValue("Tag", typeof(CompleteTag));
            this.Urls = (HashSet<DhtElement>)info.GetValue("Urls", typeof(HashSet<DhtElement>));
        }
        #region IDocumentType
        public string Id
        {
            get
            {
                if (this.Tag != null)
                {
                    return this.Tag.TagHash;
                }
                else
                {
                    return null;
                }

            }
            set
            {
            }
        }
        #endregion
        [DataMemberAttribute]
        public CompleteTag Tag
        {
            get;
            set;
        }
        [DataMemberAttribute]
        public HashSet<DhtElement> Urls
        {
            get;
            set;
        }        
        #region ILoadable

        public dynamic GetAsDatabaseType()
        {
            return this;
        }

        public bool LoadFromDatabaseType(dynamic data)
        {
            this.Tag = data.Tag;
            this.Id = data.Id;
            this.Urls.Clear();
            this.Urls.UnionWith(data.Urls);
            return true;
        }

        public Type GetDatabaseType()
        {
            return this.GetType();
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Tag", this.Tag);
            info.AddValue("Urls", this.Urls);
        }

        public bool MergeTo(KademliaResource other)
        {
            if (this.Tag.FileHash == other.Tag.FileHash)
            {
                Console.WriteLine("Merged!");
                foreach (DhtElement e in other.Urls)
                {
                    this.Urls.Add(e);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
