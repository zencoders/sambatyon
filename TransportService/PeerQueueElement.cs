using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;

namespace TransportService
{
    class PeerQueueElement
    {
        public enum ThreadState { FREE, BUSY };

        private string peerAddress;
        private float peerScore;
        private System.Timers.Timer timer;
        private ThreadState state;
        private AutoResetEvent peerQueueNotEmpty;

        public PeerQueueElement()
        {
        }

        public PeerQueueElement(string peerAddress, float peerScore, ref AutoResetEvent peerQueueNotEmpty)
        {
            this.peerAddress = peerAddress;
            this.peerScore = peerScore;
            this.state = ThreadState.FREE;
            this.peerQueueNotEmpty = peerQueueNotEmpty;
        }

        public string PeerAddress
        {
            get { return this.peerAddress; }
            set { this.peerAddress = value; }
        }

        public float PeerScore
        {
            get { return this.peerScore; }
            set { this.peerScore = value; }
        }

        public ThreadState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public void timedPeerBlock(int millis)
        {
            this.state = ThreadState.BUSY;
            this.timer = new System.Timers.Timer(millis);
            this.timer.Enabled = true;
            this.timer.Elapsed += new ElapsedEventHandler(this.TimerHandler);
        }

        public void reset()
        {
            this.state = ThreadState.FREE;
            this.peerQueueNotEmpty.Set();
        }

        private void TimerHandler(object source, EventArgs e)
        {
            this.reset();
        }
    }
}
