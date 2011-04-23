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
        KademliaBindingElement _transport;
        KademliaBindingElement _encoding;

        public NetKademliaBinding()
        {
            this._transport = new KademliaBindingElement();
            this._encoding = new KademliaBindingElement();
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
