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
    internal class PeerQueue
    {
        private Dictionary<string, PeerQueueElement> peerQueue;
        private AutoResetEvent peerQueueNotEmpty = new AutoResetEvent(true);

        public PeerQueue()
        {
        }

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

        public string GetBestPeer()
        {
            while (this.peerQueue.AsParallel().Where(p => p.Value.State == PeerQueueElement.ThreadState.FREE).Count() <= 0)
            {
                this.peerQueueNotEmpty.Reset();
                this.peerQueueNotEmpty.WaitOne();
            }
            PeerQueueElement best = this.peerQueue.AsParallel().Aggregate((l, r) => l.Value.PeerScore < r.Value.PeerScore ? l : (r.Value.PeerScore < l.Value.PeerScore ? r : (new Random().Next(2)) == 0 ? l : r )).Value;
            return best.PeerAddress;
        }

        public void ResetPeer(string key, int newScore)
        {
            this.peerQueue[key].PeerScore = newScore;
            this.peerQueue[key].Reset();
        }

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
