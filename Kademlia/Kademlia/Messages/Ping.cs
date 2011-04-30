using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Represents a ping message, used to see if a remote node is up.
	/// </summary>
	[DataContract]
	public class Ping : Message
	{
		public Ping(ID senderID, EndpointAddress nodeEndpoint) : base(senderID, nodeEndpoint)
		{
		}
		
        [DataMember]
		public override string Name
		{
            get { return "PING"; }
            set { }
		}
	}
}
