using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;

namespace UdpTransportBinding
{
    /// <summary>
    /// Binding Section for Udp. Implements configuration for SampleProfileUdpBinding.
    /// </summary>
    public class UdpBindingCollectionElement : StandardBindingCollectionElement<NetUdpBinding, UdpBindingConfigurationElement>
    {
    }
}
