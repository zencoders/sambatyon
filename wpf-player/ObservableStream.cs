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
using System.IO;

namespace wpf_player
{
    /// <summary>
    /// This class extends memory stream to allow waiting state management and position change notify.
    /// Using the <see cref="WaitForMore"/> method, a event will be launched when the needed data has been read.
    /// Position Changed event will be launched when more data has been written on the stream: the maximum resolution for this
    /// events is 1024 byte; if the position changed for less than 1024 byte the event won't be raised.
    /// </summary>
    class ObservableStream: MemoryStream
    {
        /// <summary>
        /// Waited stream position
        /// </summary>
        private long positionWaited=0L;
        /// <summary>
        /// Last position that has been lauched with a PositionChanged event
        /// </summary>
        private long lastLaunchedPosition = 0L;
        /// <summary>
        /// Flag that indicates if the stream is in waiting state
        /// </summary>
        private bool waiting=false;
        /// <summary>
        /// Event raised when waited position has been reached
        /// </summary>
        public event EventHandler WaitedPositionReached;
        /// <summary>
        /// Event raised when position change at least of 1024 byte from last notification
        /// </summary>
        public event EventHandler PositionChanged;
        /// <summary>
        /// Default constructor
        /// </summary>
        public ObservableStream()
            : base()
        {}
        /// <summary>
        /// Byte-buffer constructor
        /// </summary>
        /// <param name="buffer">Byte buffer used by the stream</param>
        public ObservableStream(byte[] buffer)
            : base(buffer)
        {}
        /// <summary>
        /// Capacity contructor. Create a buffer with the given capacity
        /// </summary>
        /// <param name="capacity">Capacity of the stream buffer</param>
        public ObservableStream(int capacity)
            : base(capacity)
        {}
        /// <summary>
        /// See MSDN Reference for Memory Stream for Details
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="writable"></param>
        public ObservableStream(byte[] buffer, bool writable)
            : base(buffer, writable)
        {}
        /// <summary>
        /// See MSDN Reference for Memory Stream for Details
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public ObservableStream(byte[] buffer,int index,int count)
            : base(buffer, index, count)
        {}
        /// <summary>
        /// See MSDN Reference for Memory Stream for Details
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="writable"></param>
        public ObservableStream(byte[] buffer, int index, int count, bool writable)
            : base(buffer, index, count, writable)
        {}
        /// <summary>
        /// See MSDN Reference for Memory Stream for Details
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="writable"></param>
        /// <param name="isVisible"></param>
        public ObservableStream(byte[] buffer, int index, int count, bool writable, bool isVisible)
            : base(buffer, index, count, writable, isVisible)
        {}
        /// <summary>
        /// This method is used to launch stream events. This performs check for waiting state and last launched position and 
        /// raise the right event in the right moment.
        /// </summary>
        private void launchEvent()
        {
            if ((waiting) && (Position >= positionWaited))
            {
                waiting = false;
                OnWaitedPositionReached(new EventArgs());
            }
            if ((Position == Length) || ((Position - lastLaunchedPosition) > 1024))
            {
                lastLaunchedPosition = Position;
                OnPositionChanges(new EventArgs());
            }
        }
        /// <summary>
        /// This method after the call of the base class implementation uses the <see cref="launchEvent"/> method to raise
        /// the right event
        /// See MSDN Reference for Memory Stream for Details.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            launchEvent();
        }
        /// <summary>
        /// This method after the call of the base class implementation uses the <see cref="launchEvent"/> method to raise
        /// the right event
        /// See MSDN Reference for Memory Stream for Details.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            launchEvent();
        }        
        /// <summary>
        /// Sets the waiting state for the stream and sets the position waited.
        /// </summary>
        /// <param name="how_many">Number of bytes needed. Default is 15000</param>
        public void WaitForMore(int how_many = 15000)
        {
            waiting = true;
            positionWaited = Math.Min(Position + how_many,Length);
        }
        /// <summary>
        /// This method calls the event handler for the waited position reached event
        /// </summary>
        /// <param name="args">Arguments that will be passed to the handler</param>
        protected virtual void OnWaitedPositionReached(EventArgs args)
        {
            EventHandler handler = WaitedPositionReached;
            if (handler != null)
            {
                handler(this, args);
            }
        }
        /// <summary>
        /// This method calls the event handler for the position changed event
        /// </summary>
        /// <param name="args">Arguments that will be passed to the handler</param>
        protected virtual void OnPositionChanges(EventArgs args)
        {
            EventHandler handler = PositionChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
