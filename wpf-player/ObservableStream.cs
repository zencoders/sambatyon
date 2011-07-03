using System;
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
