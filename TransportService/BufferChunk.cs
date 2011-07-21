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

namespace TransportService
{
    /// <summary>
    /// Internal class to the TransportService assembly. This class represents a single element
    /// of a file streamed over the network. It consists in an array of byte (the part of the stream),
    /// a state representing the chunk condition and some other structures useful to make requests over
    /// a specific chunk to die and resurrect automatically.
    /// </summary>
    internal class BufferChunk
    {
        private System.Timers.Timer timer;
        private condition actualCondition;

        /// <summary>
        /// Enumeration representing the possible states of a chunk. A chunk can have three different
        /// states: {CLEAN, DIRTY, DOWNLOADED}. A chunk in clean state is a chunk the need to be 
        /// required to the network. A chunk in dirty state is a chunk that have been required to the
        /// network a small time ago and so, to avoid network's overflows, the system knows to don't 
        /// request it again (for the moment).A chunk in downloaded state have been already correctly
        /// received by the system.
        /// </summary>
        public enum condition { CLEAN, DIRTY, DOWNLOADED };

        #region Properties
        /// <summary>
        /// Property representing the payload inside the object.
        /// </summary>
        public byte[] Payload
        {
            get;
            set;
        }

        /// <summary>
        /// Property indicating the condition of the BufferChunk. A chunk can have three different
        /// states: {CLEAN, DIRTY, DOWNLOADED}. A chunk in clean state is a chunk the need to be 
        /// required to the network. A chunk in dirty state is a chunk that have been required to the
        /// network a small time ago and so, to avoid network's overflows, the system knows to don't 
        /// request it again (for the moment).A chunk in downloaded state have been already correctly
        /// received by the system.
        /// This property in its "set" section checks if the new condition of a chunk is dirty and, if so,
        /// it enables a timer that wait for 3 seconds and if after this time the chunk is still dirty
        /// it puts the chunk in clean state.
        /// </summary>
        public condition ActualCondition
        {
            get
            {
                return this.actualCondition; 
            }

            set 
            {
                this.actualCondition = value;
                if (value == condition.DIRTY)
                {
                    this.timer = new System.Timers.Timer(3000);
                    this.timer.Enabled = true;
                    this.timer.Elapsed += new ElapsedEventHandler(this.cleaner);
                }
                else if (value == condition.CLEAN)
                {
                    if(this.timer != null)
                        this.timer.Enabled = false;
                }
            }
        }
        #endregion

        /// <summary>
        /// Default constructor of the class. It limits itself to only set the state of the chunk by default
        /// in CLEAN condition.
        /// </summary>
        public BufferChunk()
        {
            this.actualCondition = condition.CLEAN;
        }

        /// <summary>
        /// Valorized constuctor of the class. It represents a chunk already downloaded because a payload
        /// is passed to the method. The condition of the chunk is automatically set to DOWNLOADED.
        /// </summary>
        /// <param name="payload">The payload contained into the chunk.</param>
        public BufferChunk(byte[] payload)
        {
            this.Payload = payload;
            this.actualCondition = condition.DOWNLOADED;
        }

        /// <summary>
        /// Utility method used to clean the state of a chunk. It has the signature of an event method
        /// because it is passed to a timer class.
        /// </summary>
        /// <param name="source">ND</param>
        /// <param name="e">ND</param>
        private void cleaner(object source, EventArgs e)
        {
            if (this.actualCondition == condition.DIRTY)
            {
                this.actualCondition = condition.CLEAN;
            }
        }
    }
}
