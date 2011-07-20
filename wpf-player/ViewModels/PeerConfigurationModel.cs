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
using System.ComponentModel;

namespace wpf_player
{
    /// <summary>
    /// This class implements the ViewModel in the MVVM pattern. This relais data from the Peer to the 
    /// configuration windows and allows user to update this configuration
    /// </summary>
    class PeerConfigurationModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Default transport protocol UDP port
        /// </summary>
        private int udpPort=9997;
        /// <summary>
        /// Default kademlia layer UDP port
        /// </summary>
        private int kademliaPort=10000;
        /// <summary>
        /// Constructor that initializes the field with the given values
        /// </summary>
        /// <param name="udpPort">Transport Protocol UDP port</param>
        /// <param name="kademliaPort">Kademlia Layer UDP port</param>
        public PeerConfigurationModel(int udpPort, int kademliaPort)
        {
            this.udpPort = udpPort;
            this.kademliaPort = kademliaPort;
        }
        /// <summary>
        /// UDP port used by the Transport Protocol
        /// </summary>
        public int UdpPort
        {
            get
            {
                return udpPort;
            }
            set
            {
                this.udpPort = value;
                NotifyPropertyChanged("UdpPort");
            }
        }
        /// <summary>
        /// UDP port used by the kademlia netword
        /// </summary>
        public int KademliaPort
        {
            get
            {
                return kademliaPort;
            }
            set
            {
                this.kademliaPort = value;
                NotifyPropertyChanged("KademliaPort");
            }
        }
        /// <summary>
        /// Event for the property changed (this is used by WPF Data Binding)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method called when a property has been changed. This method rises the event PropertyChange.
        /// </summary>
        /// <param name="info">Name of the property that has been changed</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
