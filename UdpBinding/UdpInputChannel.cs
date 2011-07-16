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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace UdpTransportBinding
{
    class UdpInputChannel : ChannelBase, IInputChannel
    {
        MessageEncoder encoder;
        SynchronizedQueue<Message> messageQueue;

        internal UdpInputChannel(UdpChannelListener listener)
            : base(listener)
        {
            this.encoder = listener.MessageEncoderFactory.Encoder;
            this.messageQueue = new SynchronizedQueue<Message>();
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return null;
            }
        }

        //Hands the message off to other components higher up the
        //channel stack that have previously called BeginReceive() 
        //and are waiting for messages to arrive on this channel.
        internal void Dispatch(Message message)
        {
            this.messageQueue.EnqueueAndDispatch(message);
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IInputChannel))
            {
                return (T)(object)this;
            }

            T messageEncoderProperty = this.encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            return base.GetProperty<T>();
        }

        //Closes the channel ungracefully during error conditions.
        protected override void OnAbort()
        {
            this.messageQueue.Close();
        }

        //Closes the channel gracefully during normal conditions.
        protected override void OnClose(TimeSpan timeout)
        {
            this.messageQueue.Close();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
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

        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message;
            if (this.TryReceive(timeout, out message))
            {
                return message;
            }
            else
            {
                throw CreateReceiveTimedOutException(this, timeout);
            }
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginTryReceive(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return this.messageQueue.EndDequeue(result);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            UdpChannelHelpers.ValidateTimeout(timeout);
            return this.messageQueue.Dequeue(timeout, out message);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            UdpChannelHelpers.ValidateTimeout(timeout);
            return this.messageQueue.BeginDequeue(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return this.messageQueue.EndDequeue(result, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            UdpChannelHelpers.ValidateTimeout(timeout);
            return this.messageQueue.WaitForItem(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            UdpChannelHelpers.ValidateTimeout(timeout);
            return this.messageQueue.BeginWaitForItem(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.messageQueue.EndWaitForItem(result);
        }

        static TimeoutException CreateReceiveTimedOutException(IInputChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(
                    string.Format("Receive on local address {0} timed out after {1}. The time allotted to this operation may have been a portion of a longer timeout.",
                    channel.LocalAddress.Uri.AbsoluteUri, timeout));
            }
            else
            {
                return new TimeoutException(
                    string.Format("Receive timed out after {0}. The time allotted to this operation may have been a portion of a longer timeout.",
                    timeout));
            }
        }
    }
}