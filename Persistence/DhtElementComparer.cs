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
    /// Comparer Class that performs custom comparison for the HashSet containing DhtElement
    /// </summary>
    class DhtElementComparer : IEqualityComparer<DhtElement>
    {
        /// <summary>
        /// Equality Comparison that relies on DhtElement <c>Equals</c> method
        /// </summary>
        /// <param name="x">First element of the comparison</param>
        /// <param name="y">Second element of the comparison</param>
        /// <returns>True if the elements are equals, false otherwise</returns>
        bool IEqualityComparer<DhtElement>.Equals(DhtElement x, DhtElement y)
        {
            return x.Equals(y);
        }
        /// <summary>
        /// Method for getting the HashCode of an Element. This relies on DhtElement <c>GetHashCode</c> method.
        /// </summary>
        /// <param name="obj">DhtElement of whom we want to know the HashCode</param>
        /// <returns>The HashCode of the element</returns>
        int IEqualityComparer<DhtElement>.GetHashCode(DhtElement obj)
        {
            return obj.GetHashCode();
        }
    }
}
