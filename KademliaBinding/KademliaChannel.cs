using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace KademliaBinding
{
    class KademliaChannel : ChannelBase, IInputChannel
    {
        protected EndpointAddress localAddress;

        public KademliaChannel(KademliaChannelListener parent,EndpointAddress address)
            : base(parent)
        {
            this.localAddress = address;
        }

        #region IInputChannel
        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");

            return null;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");

            return null;
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            throw new Exception("The method or operation is not implemented.");

            return false;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");

            return false;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            throw new Exception("The method or operation is not implemented.");

            return false;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return ((KademliaChannelListener)this.Manager).IsMessageReady();
        }

        public IAsyncResult BeginReceive( TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public  IAsyncResult BeginReceive( AsyncCallback callback, object state)
        {
            return BeginReceive( DefaultReceiveTimeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Message Receive ( TimeSpan timeout)
        {
            //Listener gets messages from below
            return ((KademliaChannelListener)this.Manager).GetMessage();
        }

        public Message Receive ()
        {
            return Receive( DefaultReceiveTimeout);
        }

        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }
        #endregion

        #region State Machine
        protected override void OnAbort()
        {
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");

            return null;
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");
            return null;
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
    }
}
