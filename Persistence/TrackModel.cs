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
using TagLib.Mpeg;
using System.IO;
using System.Security.Cryptography;

namespace Persistence {
    /// <summary>
    /// Classe rappresentante il Modello per una traccia.
    /// </summary>
    public class TrackModel : ILoadable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TrackModel()
        {
            this.Tag = new Tag.CompleteTag();
            this.Filepath = "";
        }
        /// <summary>
        /// Constructor that initializes the Model reading the information from the file
        /// </summary>
        /// <param name="filename">Filename where to read the information</param>
        public TrackModel(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                FileInfo finfo = new FileInfo(filename);
                this.Tag = new Tag.CompleteTag(finfo.FullName);
                this.Filepath = finfo.FullName;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        /// <summary>
        /// Hash of the File
        /// </summary>
        public string Hash
        {
            get
            {
                return Tag.FileHash;
            }
        }
        /// <summary>
        /// Path of the file
        /// </summary>
        public string Filepath
        {
            get;
            private set;
        }
        public Tag.CompleteTag Tag {
            get;
            private set;
        }


        #region ILoadable
        /// <summary>
        /// Track Database Type. This subtype is used when the object is loaded or stored in a repository.
        /// </summary>
        public class Track:IDocumentType
        {
            /// <summary>
            /// Track Identifier
            /// </summary>
            public string Id
            {
                get;
                set;
            }
            /// <summary>
            /// Track filename
            /// </summary>
            public string Filename
            {
                get;
                set;
            }
        }
        /// <summary>
        /// Returns the object as Database Type.
        /// </summary>
        /// <returns></returns>
        public dynamic GetAsDatabaseType()
        {
            return new Track
            {
                Id = this.Hash,
                Filename = this.Filepath
            };
        }
        /// <summary>
        /// Loads data from dynamic object loaded from repository
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if the data has been successfully loaded, false otherwise</returns>
        public bool LoadFromDatabaseType(dynamic data)
        {
            Tag.CompleteTag t = new Tag.CompleteTag(data.Filename);
            if (t.FileHash.Equals(data.Id))
            {
                this.Tag = t;
                this.Filepath = data.Filename;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Gets type reference to the database type
        /// </summary>
        /// <returns></returns>
        public Type GetDatabaseType()
        {
            return typeof(Track);
        }
        #endregion
        
    }
}
