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
    internal class BufferChunk
    {
        private System.Timers.Timer timer;
        private condition actualCondition;
        public enum condition { CLEAN, DIRTY, DOWNLOADED };

        public byte[] Payload
        {
            get;
            set;
        }

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

        private void cleaner(object source, EventArgs e)
        {
            if (this.actualCondition == condition.DIRTY)
            {
                this.actualCondition = condition.CLEAN;
            }
        }

        public BufferChunk()
        {
            this.actualCondition = condition.CLEAN;
        }

        public BufferChunk(byte[] payload)
        {
            this.Payload = payload;
            this.actualCondition = condition.DOWNLOADED;
        }
    }
}
