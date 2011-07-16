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

﻿#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Globalization;
#endregion

namespace UdpTransportBinding
{
    /// <summary>
    /// IChannelFactory implementation for Udp.
    /// 
    /// Supports IOutputChannel only, as Udp is fundamentally
    /// a datagram protocol.
    /// </summary>
    class UdpChannelFactory : ChannelFactoryBase<IOutputChannel>
    {
        #region member_variables
        BufferManager bufferManager;
        MessageEncoderFactory messageEncoderFactory;
        bool multicast;
        #endregion

        internal UdpChannelFactory(UdpTransportBindingElement bindingElement, BindingContext context)
            : base(context.Binding)
        {
            this.multicast = bindingElement.Multicast;
            this.bufferManager = BufferManager.CreateBufferManager(bindingElement.MaxBufferPoolSize, int.MaxValue);

            Collection<MessageEncodingBindingElement> messageEncoderBindingElements
                = context.BindingParameters.FindAll<MessageEncodingBindingElement>();

            if (messageEncoderBindingElements.Count > 1)
            {
                throw new InvalidOperationException("More than one MessageEncodingBindingElement was found in the BindingParameters of the BindingContext");
            }
            else if (messageEncoderBindingElements.Count == 1)
            {
                this.messageEncoderFactory = messageEncoderBindingElements[0].CreateMessageEncoderFactory();
            }
            else
            {
                this.messageEncoderFactory = UdpConstants.DefaultMessageEncoderFactory;
            }
        }

        public BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return this.messageEncoderFactory;
            }
        }

        public bool Multicast
        {
            get
            {
                return this.multicast;
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

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        /// <summary>
        /// Create a new Udp Channel. Supports IOutputChannel.
        /// </summary>
        /// <typeparam name="TChannel">The type of Channel to create (e.g. IOutputChannel)</typeparam>
        /// <param name="remoteAddress">The address of the remote endpoint</param>
        /// <returns></returns>
        protected override IOutputChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            return new UdpOutputChannel(this, remoteAddress, via, MessageEncoderFactory.Encoder);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.bufferManager.Clear();
        }
    }
}