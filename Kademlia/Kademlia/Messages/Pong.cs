using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Represents a ping reply.
	/// </summary>
	[DataContract]
	public class Pong : Response
	{
		public Pong(ID senderID, Ping ping, EndpointAddress nodeEndpoint) : base(senderID, ping, nodeEndpoint)
		{
		}
		
        [DataMember]
		public override string Name
		{
            get { return "PONG"; }
            set { }
		}
	}
}
