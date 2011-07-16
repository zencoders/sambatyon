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
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WsdlNS = System.Web.Services.Description;
using System.Xml;

namespace UdpTransportBinding
{
    /// <summary>
    /// Policy import/export for Udp
    /// </summary>
    public class UdpBindingElementImporter : IPolicyImportExtension, IWsdlImportExtension
    {
        public UdpBindingElementImporter()
        {
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw new ArgumentNullException("importer");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            UdpTransportBindingElement udpBindingElement = null;
            bool multicast = false;
            PolicyAssertionCollection policyAssertions = context.GetBindingAssertions();
            if (policyAssertions.Remove(UdpPolicyStrings.TransportAssertion, UdpPolicyStrings.UdpNamespace) != null)
            {
                udpBindingElement = new UdpTransportBindingElement();
            }
            if (policyAssertions.Remove(UdpPolicyStrings.MulticastAssertion, UdpPolicyStrings.UdpNamespace) != null)
            {
                multicast = true;
            }
            if (udpBindingElement != null)
            {
                udpBindingElement.Multicast = multicast;
                context.BindingElements.Add(udpBindingElement);
            }
        }

        public void BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments,
                                 System.Xml.Schema.XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        { }

        public void ImportContract(WsdlImporter importer, WsdlContractConversionContext context)
        { }

        public void ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.Endpoint.Binding == null)
            {
                throw new ArgumentNullException("context.Endpoint.Binding");
            }

            BindingElementCollection bindingElements = context.Endpoint.Binding.CreateBindingElements();
            TransportBindingElement transportBindingElement = bindingElements.Find<TransportBindingElement>();
            if (transportBindingElement is UdpTransportBindingElement)
            {
                ImportAddress(context);
            }

            if (context.Endpoint.Binding is CustomBinding)
            {
                Binding binding;

                if (transportBindingElement is UdpTransportBindingElement)
                {
                    //if TryCreate is true, the CustomBinding will be replace by a SampleProfileUdpBinding in the
                    //generated config file for better typed generation.
                    if (NetUdpBinding.TryCreate(bindingElements, out binding))
                    {
                        binding.Name = context.Endpoint.Binding.Name;
                        binding.Namespace = context.Endpoint.Binding.Namespace;
                        context.Endpoint.Binding = binding;
                    }
                }
            }
        }

        //this imports the address of the endpoint.
        void ImportAddress(WsdlEndpointConversionContext context)
        {
            EndpointAddress address = null;

            if (context.WsdlPort != null)
            {
                XmlElement addressing10Element =
                    context.WsdlPort.Extensions.Find("EndpointReference", AddressingVersionConstants.WSAddressing10NameSpace);

                XmlElement addressing200408Element =
                    context.WsdlPort.Extensions.Find("EndpointReference", AddressingVersionConstants.WSAddressingAugust2004NameSpace);

                WsdlNS.SoapAddressBinding soapAddressBinding =
                    (WsdlNS.SoapAddressBinding)context.WsdlPort.Extensions.Find(typeof(WsdlNS.SoapAddressBinding));

                if (addressing10Element != null)
                {
                    address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressing10,
                                                       new XmlNodeReader(addressing10Element));
                }
                if (addressing200408Element != null)
                {
                    address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressingAugust2004,
                                                       new XmlNodeReader(addressing200408Element));
                }
                else if (soapAddressBinding != null)
                {
                    // checking for soapAddressBinding checks for both Soap 1.1 and Soap 1.2
                    address = new EndpointAddress(soapAddressBinding.Location);
                }
            }

            if (address != null)
            {
                context.Endpoint.Address = address;
            }
        }
    }
}
