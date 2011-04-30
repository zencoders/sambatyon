using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;

namespace UdpBinding
{
    /// <summary>
    /// Binding Section for Udp. Implements configuration for SampleProfileUdpBinding.
    /// </summary>
    public class SampleProfileUdpBindingCollectionElement : StandardBindingCollectionElement<NetUdpBinding, UdpBindingConfigurationElement>
    {
    }
}
