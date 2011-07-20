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
using System.Reflection;

namespace Persistence
{
    /// <summary>
    /// Class that implements a generic container for configurations used by the repository
    /// </summary>
    public class RepositoryConfiguration: System.Collections.Generic.Dictionary<string,string>
    {
        /// <summary>
        /// Default Constructor that initializes an empty configuration
        /// </summary>
        public RepositoryConfiguration(): base() {}
        /// <summary>
        /// Constructor that initializes the configuration dictionary using an anonymous object.
        /// </summary>
        /// <param name="anon">Anonymous object used to build the configuration</param>
        public RepositoryConfiguration(Object anon)
            : this()
        {
            PropertyInfo[] props = anon.GetType().GetProperties();
            for (int k = 0; k < props.Length; k++)
            {
                this.SetConfig(props[k].Name ,props[k].GetValue(anon, null).ToString());
            }
        }
        /// <summary>
        /// Gets the configuration associated with a key.
        /// </summary>
        /// <param name="key">Name of the configuration to get</param>
        /// <returns>The value of the configuration if it is present, empty string otherwise</returns>
        public string GetConfig(string key)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Sets the configuration 
        /// </summary>
        /// <param name="key">Name of the configuration to set</param>
        /// <param name="value">The new value of the configuration</param>
        public void SetConfig(string key, string value)
        {
            this.Add(key, value);
        }            
    }
}
