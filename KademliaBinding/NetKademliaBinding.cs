using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace KademliaBinding
{
    public class NetKademliaBinding : Binding
    {
        public NetKademliaBinding()
        {
            Initialize();
        }
        void Initialize()
        {
 /*           transport = new UdpTransportBindingElement();
            session = new ReliableSessionBindingElement();
            compositeDuplex = new CompositeDuplexBindingElement();
            encoding = new TextMessageEncodingBindingElement();*/
        }
    }
}
