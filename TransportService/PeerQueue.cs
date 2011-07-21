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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TransportService
{
    /// <summary>
    /// Class representing the queue of peer recognized as senders of a particular resource.
    /// It works to allow to balance load over peer while chosing every time the best peer to request to.
    /// This class is strictly connected to its foundamentals elements represented by theclass PeerQueueElement.
    /// </summary>
    /// <seealso cref="TransportService.PeerQueueElement"/>
    internal class PeerQueue
    {
        private Dictionary<string, PeerQueueElement> peerQueue;
        private AutoResetEvent peerQueueNotEmpty = new AutoResetEvent(true);

        /// <summary>
        /// Default constructor of the class.
        /// </summary>
        public PeerQueue()
        {
        }

        /// <summary>
        /// Constructor of the class used to actively insert a certain number of peer inside the queue.
        /// </summary>
        /// <param name="peerQueue">
        /// A dictionay representing a bunch of peer each associated with a score of the peer. The less is
        /// the score, the better is the peer.
        /// </param>
        public PeerQueue(Dictionary<string, float> peerQueue)
        {
            //this.peerQueue = new PeerQueueElement[peerQueue.Count];
            //var rangePartitioner = Partitioner.Create(0, peerQueue.Count);
            this.peerQueue = new Dictionary<string,PeerQueueElement>();
/*            Parallel.ForEach(peerQueue, p =>
            {
                this.peerQueue[p.Key] = new PeerQueueElement(p.Key, p.Value, ref peerQueueNotEmpty);
            });*/
            foreach (string key in peerQueue.Keys)
            {
                this.peerQueue[key] = new PeerQueueElement(key, peerQueue[key], ref peerQueueNotEmpty);
            }
        }

        /// <summary>
        /// Method used to get the best sender peer at the moment of the call. 
        /// The best peer of the queue is the FREE peer that has the lesser score.
        /// This method is blocking until there are no FREE peer into the network.
        /// </summary>
        /// <returns>A string representing the URI address of the transport service of the best peer.</returns>
        public string GetBestPeer()
        {
            while (this.peerQueue.AsParallel().Where(p => p.Value.State == PeerQueueElement.PeerState.FREE).Count() <= 0)
            {
                this.peerQueueNotEmpty.Reset();
                this.peerQueueNotEmpty.WaitOne();
            }
            PeerQueueElement best = this.peerQueue.AsParallel().Aggregate((l, r) => l.Value.PeerScore < r.Value.PeerScore ? l : (r.Value.PeerScore < l.Value.PeerScore ? r : (new Random().Next(2)) == 0 ? l : r )).Value;
            return best.PeerAddress;
        }

        /// <summary>
        /// Method used to reset the state of a single peer into the queue. It sets the new score (as passed)
        /// of the peer indicated.
        /// </summary>
        /// <param name="key">The URI representing the peer.</param>
        /// <param name="newScore">The new score of the peer.</param>
        public void ResetPeer(string key, int newScore)
        {
            this.peerQueue[key].PeerScore = newScore;
            this.peerQueue[key].Reset();
        }

        /// <summary>
        /// Operator overload of the operator square brackets. It directly refers to the inner dictionary.
        /// </summary>
        /// <param name="key">The URI of the peer to select</param>
        /// <returns>The PeerQueueElement object indicated in dictionary by the key.</returns>
        public PeerQueueElement this[string key]
        {
            get
            {
                return this.peerQueue[key];
            }
            set
            {
                this.peerQueue[key] = value;
            }
        }
    }
}
