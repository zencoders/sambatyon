using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;

namespace KademliaBinding
{
    public class KademliaBindingConfigurationElement : StandardBindingElement
    {
        public KademliaBindingConfigurationElement(string configurationName)
            : base(configurationName)
        {
        }

        public KademliaBindingConfigurationElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetKademliaBinding); }
        }
    }
}
