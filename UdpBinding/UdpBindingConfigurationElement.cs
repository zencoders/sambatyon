using System;
using System.ServiceModel.Channels;
using System.Configuration;
using System.Collections;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace UdpTransportBinding
{
    public class UdpBindingConfigurationElement : StandardBindingElement
    {
        public UdpBindingConfigurationElement(string configurationName)
            : base(configurationName)
        {
        }

        public UdpBindingConfigurationElement()
            : this(null)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(NetUdpBinding); }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            if (binding.GetType() != typeof(NetUdpBinding))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "Invalid type for binding. Expected type: {0}. Type passed in: {1}.",
                    typeof(NetUdpBinding).AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }
        }

    }
}
