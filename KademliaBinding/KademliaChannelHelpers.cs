using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Globalization;

namespace KademliaBinding
{
    static class KademliaConstants
    {
   //     internal const string EventLogSourceName = "Microsoft.ServiceModel.Samples";
        internal const string Scheme = "soap.udp";
        internal const string KademliaBindingSectionName = "system.serviceModel/bindings/netKademliaBinding";
        internal const string KademliaTransportSectionName = "kademliaTransport";

        static MessageEncoderFactory messageEncoderFactory;
        static KademliaConstants()
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
            get { return KademliaConstants.Scheme + "://address"; }
        }
    }
}
