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
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Globalization;

namespace UdpTransportBinding
{
    static class UdpConstants
    {
        internal const string Scheme = "soap.udp";
        internal const string UdpBindingSectionName = "system.serviceModel/bindings/netUdpBinding";
        internal const string UdpTransportSectionName = "udpTransport";

        static MessageEncoderFactory messageEncoderFactory;
        static UdpConstants()
        {
            messageEncoderFactory = new TextMessageEncodingBindingElement().CreateMessageEncoderFactory();
        }

        // ensure our advertised MessageVersion matches the version we're
        // using to serialize/deserialize data to/from the wire
        internal static MessageVersion MessageVersion
        {
            get
            {
                return messageEncoderFactory.MessageVersion;
            }
        }

        // we can use the same encoder for all our Udp Channels as it's free-threaded
        internal static MessageEncoderFactory DefaultMessageEncoderFactory
        {
            get
            {
                return messageEncoderFactory;
            }
        }

        public static string Uri
        {
            get { return UdpConstants.Scheme + "://address"; }
        }
    }

    static class UdpConfigurationStrings
    {
        public const string MaxBufferPoolSize = "maxBufferPoolSize";
        public const string MaxReceivedMessageSize = "maxMessageSize";
        public const string Multicast = "multicast";
        public const string OrderedSession = "orderedSession";
        public const string ReliableSessionEnabled = "reliableSessionEnabled";
        public const string SessionInactivityTimeout = "sessionInactivityTimeout";
        public const string ClientBaseAddress = "clientBaseAddress";
    }

    static class UdpPolicyStrings
    {
        public const string UdpNamespace = "http://sample.schemas.microsoft.com/policy/udp";
        public const string Prefix = "udp";
        public const string MulticastAssertion = "Multicast";
        public const string TransportAssertion = "soap.udp";
    }

    static class UdpChannelHelpers
    {
        /// <summary>
        /// The Channel layer normalizes exceptions thrown by the underlying networking implementations
        /// into subclasses of CommunicationException, so that Channels can be used polymorphically from
        /// an exception handling perspective.
        /// </summary>
        internal static CommunicationException ConvertTransferException(SocketException socketException)
        {
            return new CommunicationException(
                string.Format(CultureInfo.CurrentCulture, 
                "A Udp error ({0}: {1}) occurred while transmitting data.", socketException.ErrorCode, socketException.Message), 
                socketException);
        }

        internal static bool IsInMulticastRange(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                // 224.0.0.0 through 239.255.255.255
                byte[] addressBytes = address.GetAddressBytes();
                return ((addressBytes[0] & 0xE0) == 0xE0);
                //(address.Address & MulticastIPAddress.IPv4MulticastMask) == MulticastIPAddress.IPv4MulticastMask);
            }
            else
            {
                return address.IsIPv6Multicast;
            }
        }

        internal static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout must be greater than or equal to TimeSpan.Zero. To disable timeout, specify TimeSpan.MaxValue.");
            }
        }
    }

    static class UdpDefaults
    {
        internal const long MaxBufferPoolSize = 64 * 1024;
        internal const int MaxReceivedMessageSize = 5 * 1024 * 1024; //64 * 1024;
        internal const bool Multicast = false;
        internal const bool OrderedSession = true;
        internal const bool ReliableSessionEnabled = true;
        internal const string SessionInactivityTimeoutString = "00:10:00";
    }

    static class AddressingVersionConstants
    {
        internal const string WSAddressing10NameSpace = "http://www.w3.org/2005/08/addressing";
        internal const string WSAddressingAugust2004NameSpace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
    }
}
