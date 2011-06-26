using System;
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
        public enum ThreadState { FREE, BUSY };

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
            this.State = ThreadState.FREE;
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

        public ThreadState State
        {
            get;
            set;
        }

        public void GetChunk(ChunkRequest chkrq)
        {
            this.State = ThreadState.BUSY;
            this.TimedPeerBlock(3000);
            this.channel.GetChunk(chkrq);
        }

        public void TimedPeerBlock(int millis)
        {
            this.State = ThreadState.BUSY;
            this.timer = new System.Timers.Timer(millis);
            this.timer.Enabled = true;
            this.timer.Elapsed += new ElapsedEventHandler(this.timerHandler);
        }

        public void Reset()
        {
            this.State = ThreadState.FREE;
            if(this.timer != null)
                this.timer.Enabled = false;
            this.peerQueueNotEmpty.Set();
        }

        private void timerHandler(object source, EventArgs e)
        {
            this.Reset();
        }
    }
}
