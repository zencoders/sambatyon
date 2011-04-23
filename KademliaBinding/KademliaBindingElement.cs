using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace KademliaBinding
{
    public class KademliaBindingElement : TransportBindingElement//, IPolicyExportExtension, IWsdlExportExtension
    {
        EndpointAddress _address;

        public KademliaBindingElement()
        {
            _address = new EndpointAddress(KademliaConstants.Uri);
        }

        protected KademliaBindingElement(KademliaBindingElement other)
            : base(other)
        {
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (!CanBuildChannelFactory<TChannel>(context))
            {
                throw new ArgumentException(String.Format("Unsupported channel type: {0}.", typeof(TChannel).Name));
            }
            // return (IChannelFactory<TChannel>)(object)new KademliaChannelFactory(this, context);
            return null;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unsupported channel type: {0}.", typeof(TChannel).Name));
            }
            return (IChannelListener<TChannel>)(object)new KademliaChannelListener(this, context);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            // return context.CanBuildInnerChannelFactory<TChannel>();
            return (typeof(TChannel) == typeof(IOutputChannel));
        }

        /// <summary>
        /// Used by higher layers to determine what types of channel listeners this
        /// binding element supports. Which in this case is just IInputChannel.
        /// </summary>
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            // return context.CanBuildInnerChannelListener<TChannel>();
            return (typeof(TChannel) == typeof(IInputChannel));
        }

        public override string Scheme
        {
            get
            {
                return KademliaConstants.Scheme;
            }
        }

        public override BindingElement Clone()
        {
            return new KademliaBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return context.GetInnerProperty<T>();
        }

        public EndpointAddress LocalAddress
        {
            get { return this._address; }
        }
    }
}
