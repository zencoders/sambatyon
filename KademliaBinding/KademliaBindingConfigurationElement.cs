using System;
using System.ServiceModel.Channels;
using System.Configuration;
using System.Collections;
using System.Globalization;
using System.ServiceModel;
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

        protected override void OnApplyConfiguration(Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            if (binding.GetType() != typeof(NetKademliaBinding))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "Invalid type for binding. Expected type: {0}. Type passed in: {1}.",
                    typeof(NetKademliaBinding).AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }
        }

    }
}
