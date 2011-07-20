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
    /// Interface that describe an object that can be loaded in the the repository.
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// This method is used to return a instance of an object that contains only information that
        /// has to be serialized.
        /// </summary>
        /// <returns>A representation as database type of the current object</returns>
        dynamic GetAsDatabaseType();
        /// <summary>
        /// Load data from an object loaded from the repository as Database type
        /// </summary>
        /// <param name="data">Dynamic object containing data</param>
        /// <returns>True if all data have been successfully loaded, false otherwise</returns>
        bool LoadFromDatabaseType(dynamic data);
        /// <summary>
        /// Get database type associated to this object
        /// </summary>
        /// <returns>Type reference for the Database Type</returns>
        Type GetDatabaseType();
    }
}
