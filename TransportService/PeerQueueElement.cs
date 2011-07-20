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
using System.Timers;
using System.Threading;
using System.ServiceModel;
using UdpTransportBinding;
using TransportService.Messages;

namespace TransportService
{
    internal class PeerQueueElement
    {
        public enum PeerState { FREE, BUSY };

        private System.Timers.Timer timer;
        private AutoResetEvent peerQueueNotEmpty;
        private ITransportProtocol channel;

        public PeerQueueElement()
        {
        }

        public PeerQueueElement(string peerAddress, float peerScore, ref AutoResetEvent peerQueueNotEmpty)
        {
            this.PeerAddress = peerAddress;
            this.PeerScore = peerScore;
            this.State = PeerState.FREE;
            this.peerQueueNotEmpty = peerQueueNotEmpty;
            this.channel = ChannelFactory<ITransportProtocol>.CreateChannel(
                    new NetUdpBinding(), new EndpointAddress(peerAddress)
            );
        }

        public string PeerAddress
        {
            get;
            set;
        }

        public float PeerScore
        {
            get;
            set;
        }

        public PeerState State
        {
            get;
            set;
        }

        public void GetChunk(ChunkRequest chkrq)
        {
            this.State = PeerState.BUSY;
            this.TimedPeerBlock(3000);
            this.channel.GetChunk(chkrq);
        }

        public void TimedPeerBlock(int millis)
        {
            this.State = PeerState.BUSY;
            this.timer = new System.Timers.Timer(millis);
            this.timer.Enabled = true;
            this.timer.Elapsed += new ElapsedEventHandler(this.timerHandler);
        }

        public void Reset()
        {
            this.State = PeerState.FREE;
            if(this.timer != null)
                this.timer.Enabled = false;
            this.peerQueueNotEmpty.Set();
        }

        private void timerHandler(object source, EventArgs e)
        {
            this.PeerScore += 1;
            this.Reset();
        }
    }
}
