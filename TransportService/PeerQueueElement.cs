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
    /// <summary>
    /// This class represent an abstraction of the peer from the point of view of a requestor. It is used
    /// inside a PeerQueue.
    /// The class internally stores a channel used to communicate with other peer. It is useful to avoid
    /// opening and closing the channel.
    /// This class is strictly coupled with PeerQueue.
    /// </summary>
    /// <seealso cref="TransportService.PeerQueue"/>
    internal class PeerQueueElement
    {
        /// <summary>
        /// Enumeration representing all possible states that a PeerQueueElement (foundamentally an
        /// abstraction of the peer in the network) can assume.
        /// Actually there are two states. A peer may be FREE, and so it is possible to use it to request
        /// resources, or BUSY that indicates that the peer have been recently asked and has not yet answered.
        /// </summary>
        public enum PeerState { FREE, BUSY };

        private System.Timers.Timer timer;
        private AutoResetEvent peerQueueNotEmpty;
        private ITransportProtocol channel;

        /// <summary>
        /// Default constructor of the class.
        /// </summary>
        public PeerQueueElement()
        {
        }

        /// <summary>
        /// Constructor of the class that effectively valorize its attributes.
        /// </summary>
        /// <param name="peerAddress">URI address of the peer</param>
        /// <param name="peerScore">Score of the peer (the less it is the better is the peer</param>
        /// <param name="peerQueueNotEmpty">
        /// AutoResetEvent used to communicate to the Peer Queue that the queue is not empty.
        /// </param>
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

        /// <summary>
        /// Property representing the address of the peer.
        /// </summary>
        public string PeerAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Property representing the score of the peer. The less it is the better is the peer.
        /// </summary>
        public float PeerScore
        {
            get;
            set;
        }

        /// <summary>
        /// Property representing the state of the peer according to the condition enum.
        /// </summary>
        public PeerState State
        {
            get;
            set;
        }

        /// <summary>
        /// Method used to require a chunk to the peer represented by the inner channel.
        /// </summary>
        /// <param name="chkrq">ChunkRequest object containing informations about the resource.</param>
        /// <seealso cref="TransportService.Messages.ChunkRequest"/>
        public void GetChunk(ChunkRequest chkrq)
        {
            this.State = PeerState.BUSY;
            this.TimedPeerBlock(3000);
            this.channel.GetChunk(chkrq);
        }

        /// <summary>
        /// Method used to block the peer (avoid to request again) for a certain number of milliseconds as
        /// passed by argument. For the time indicated the peer is put in the BUSY state; after that time
        /// the peer is resettled in FREE state.
        /// </summary>
        /// <param name="millis">time used to block the peer.</param>
        public void TimedPeerBlock(int millis)
        {
            this.State = PeerState.BUSY;
            this.timer = new System.Timers.Timer(millis);
            this.timer.Enabled = true;
            this.timer.Elapsed += new ElapsedEventHandler(this.timerHandler);
        }

        /// <summary>
        /// Method actively used to reset a peer putting it in free state. If the peer have a timer active
        /// the timer is disabled.
        /// </summary>
        public void Reset()
        {
            this.State = PeerState.FREE;
            if(this.timer != null)
                this.timer.Enabled = false;
            this.peerQueueNotEmpty.Set();
        }

        /// <summary>
        /// Method called by the timer to automatically call the Reset method. Each time this method
        /// is called the peer score is augmented by one.
        /// </summary>
        /// <param name="source">ND</param>
        /// <param name="e">ND</param>
        private void timerHandler(object source, EventArgs e)
        {
            this.PeerScore += 1;
            this.Reset();
        }
    }
}
