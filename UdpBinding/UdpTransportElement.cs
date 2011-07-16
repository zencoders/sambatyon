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

﻿#region using
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
#endregion

namespace UdpTransportBinding
{
    /// <summary>
    /// Configuration section for Udp. 
    /// </summary>
    public class UdpTransportElement : BindingElementExtensionElement
    {
        public UdpTransportElement()
        {
        }

        #region Configuration_Properties
        [ConfigurationProperty(UdpConfigurationStrings.MaxBufferPoolSize, DefaultValue = UdpDefaults.MaxBufferPoolSize)]
        [LongValidator(MinValue = 0)]
        public long MaxBufferPoolSize
        {
            get { return (long)base[UdpConfigurationStrings.MaxBufferPoolSize]; }
            set { base[UdpConfigurationStrings.MaxBufferPoolSize] = value; }
        }

        [ConfigurationProperty(UdpConfigurationStrings.MaxReceivedMessageSize, DefaultValue = UdpDefaults.MaxReceivedMessageSize)]
        [IntegerValidator(MinValue = 1)]
        public int MaxReceivedMessageSize
        {
            get { return (int)base[UdpConfigurationStrings.MaxReceivedMessageSize]; }
            set { base[UdpConfigurationStrings.MaxReceivedMessageSize] = value; }
        }

        [ConfigurationProperty(UdpConfigurationStrings.Multicast, DefaultValue = UdpDefaults.Multicast)]
        public bool Multicast
        {
            get { return (bool)base[UdpConfigurationStrings.Multicast]; }
            set { base[UdpConfigurationStrings.Multicast] = value; }
        }
        #endregion

        public override Type BindingElementType
        {
            get { return typeof(UdpTransportBindingElement); }
        }

        protected override BindingElement CreateBindingElement()
        {
            UdpTransportBindingElement bindingElement = new UdpTransportBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        #region Configuration_Infrastructure_overrides
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);

            UdpTransportBindingElement udpBindingElement = (UdpTransportBindingElement)bindingElement;
            udpBindingElement.MaxBufferPoolSize = this.MaxBufferPoolSize;
            udpBindingElement.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            udpBindingElement.Multicast = this.Multicast;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            UdpTransportElement source = (UdpTransportElement)from;
            this.MaxBufferPoolSize = source.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = source.MaxReceivedMessageSize;
            this.Multicast = source.Multicast;
        }

        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);

            UdpTransportBindingElement udpBindingElement = (UdpTransportBindingElement)bindingElement;
            this.MaxBufferPoolSize = udpBindingElement.MaxBufferPoolSize;
            this.MaxReceivedMessageSize = (int)udpBindingElement.MaxReceivedMessageSize;
            this.Multicast = udpBindingElement.Multicast;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection properties = base.Properties;
                properties.Add(new ConfigurationProperty(UdpConfigurationStrings.MaxBufferPoolSize,
                    typeof(long), UdpDefaults.MaxBufferPoolSize, null, new LongValidator(0, Int64.MaxValue), ConfigurationPropertyOptions.None));
                properties.Add(new ConfigurationProperty(UdpConfigurationStrings.MaxReceivedMessageSize,
                    typeof(int), UdpDefaults.MaxReceivedMessageSize, null, new IntegerValidator(1, Int32.MaxValue), ConfigurationPropertyOptions.None));
                properties.Add(new ConfigurationProperty(UdpConfigurationStrings.Multicast,
                    typeof(Boolean), UdpDefaults.Multicast, null, null, ConfigurationPropertyOptions.None));
                return properties;
            }
        }
        #endregion
    }
}