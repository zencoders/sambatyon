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

namespace Persistence
{
    /// <summary>
    /// Class used to represent a keyword inside Kademlia Network. A kademlia keyword has one or more tags associated.
    /// </summary>
    public class KademliaKeyword: IDocumentType, ILoadable
    {
        /// <summary>
        /// List containing the identifiers (hash) of the tags releated to the keyword
        /// </summary>
        public List<string> Tags
        {
            get;
            set;
        }
        /// <summary>
        /// Default keyword constructor. 
        /// </summary>
        public KademliaKeyword()
        {
            this.Id = null;
            this.Tags = new List<string>();
        }
        /// <summary>
        /// Main keyword constructor. This just initializes the fields with the given values
        /// </summary>
        /// <param name="key">keyword itself used as identifier</param>
        /// <param name="tags">identifier of associated tags</param>
        public KademliaKeyword(string key, params string[] tags)
            : this()
        {
            this.Id = key;
            this.Tags.AddRange(tags);
        }
        #region ILoadable 
        /// <summary>
        /// Returns the object itself because it's ready to be loaded in the repository (and implements IDocumentType!)
        /// </summary>
        /// <returns>The object itself</returns>
        public dynamic GetAsDatabaseType()
        {
            return this;
        }
        /// <summary>
        /// Loads keyword information from a dynamic object loaded from repository
        /// </summary>
        /// <param name="data">Data Object loaded from repository</param>
        /// <returns>Always True</returns>
        public bool LoadFromDatabaseType(dynamic data)
        {
            this.Id = data.Id;
            Tags.Clear();
            this.Tags.AddRange(data.Tags);
            return true;
        }

        /// <summary>
        /// Returns the Type of the KadmeliaKeyword class.
        /// </summary>
        /// <returns>The type used for Repository</returns>
        public Type GetDatabaseType()
        {
            return this.GetType();
        }

        #endregion

        #region IDocumentType 

        /// <summary>
        /// Keyword identifier is the keyword itself.
        /// <example>Keyword <c>fish</c> will have <c>fish</c> as identifier inside the repository</example>
        /// </summary>
        public string Id
        {
            get;
            set;
        }
        #endregion
    }
}
