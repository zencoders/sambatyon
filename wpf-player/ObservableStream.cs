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
    class ObservableStream: MemoryStream
    {
        private long positionWaited=0L;
        private long lastLaunchedPosition = 0L;
        private bool waiting=false;
        public event EventHandler WaitedPositionReached;
        public event EventHandler PositionChanged;
        public ObservableStream()
            : base()
        {}
        public ObservableStream(byte[] buffer)
            : base(buffer)
        {}
        public ObservableStream(int capacity)
            : base(capacity)
        {}
        public ObservableStream(byte[] buffer, bool writable)
            : base(buffer, writable)
        {}
        public ObservableStream(byte[] buffer,int index,int count)
            : base(buffer, index, count)
        {}
        public ObservableStream(byte[] buffer, int index, int count, bool writable)
            : base(buffer, index, count, writable)
        {}
        public ObservableStream(byte[] buffer, int index, int count, bool writable, bool isVisible)
            : base(buffer, index, count, writable, isVisible)
        {}
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
        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            launchEvent();
        }
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            launchEvent();
        }        
        public void WaitForMore(int how_many = 15000)
        {
            waiting = true;
            positionWaited = Math.Min(Position + how_many,Length);
        }
        protected virtual void OnWaitedPositionReached(EventArgs args)
        {
            EventHandler handler = WaitedPositionReached;
            if (handler != null)
            {
                handler(this, args);
            }
        }
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
