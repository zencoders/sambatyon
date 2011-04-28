using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace UdpBinding
{
    public class NetUdpBinding : Binding
    {
        UdpBindingElement _transport;
        UdpBindingElement _encoding;

        public NetUdpBinding()
        {
            this._transport = new UdpBindingElement();
            this._encoding = new UdpBindingElement();
        }

        public override string Scheme { get { return KademliaConstants.Scheme; } }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();

            bindingElements.Add(this._encoding);
            bindingElements.Add(this._transport);

            return bindingElements.Clone();
        }
    }
}
