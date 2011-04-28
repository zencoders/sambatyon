#region using
using System;
using System.ServiceModel.Dispatcher;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.ServiceModel.Description;
using Kademlia;
#endregion

namespace UdpBinding
{
    class UdpChannelListener : ChannelListenerBase<IInputChannel>
    {
        Uri _uri;
        MessageEncoderFactory messageEncoderFactory;
        private Kademlia.KademliaNode dhtNode;
        private EndpointAddress _localAddress;

        public UdpChannelListener(UdpBindingElement transportElement, BindingContext context)
            : base(context.Binding)
        {

            _uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);

            _localAddress = transportElement.LocalAddress;
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return messageEncoderFactory;
            }
        }

        string Scheme
        {
            get
            {
                return KademliaConstants.Scheme;
            }
        }

        public override T GetProperty<T>()
        {
            T messageEncoderProperty = this.MessageEncoderFactory.Encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)this.MessageEncoderFactory.Encoder.MessageVersion;
            }

            return base.GetProperty<T>();
        }

        #region Listener Base
        protected override IInputChannel OnAcceptChannel(TimeSpan timeout)
        {
            IInputChannel trans = null;

            trans = new UdpChannel(this, _localAddress);

            return trans;
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override IInputChannel OnEndAcceptChannel(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override Uri Uri
        {
            get
            {
                return this._uri;
            }
        }
        #endregion

        #region State Machine
        protected override void OnOpen(TimeSpan timeout)
        {

        }

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

        #region Specific
        public bool IsMessageReady()
        {
            //TODO: Implementare il metodo
            return true;
        }

        public Message GetMessage()
        {
            //TODO: Implementare il metodo
            Message msg = null;
            return msg;
        }
        #endregion
    }
}