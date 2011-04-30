using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Represents a response message, in the same conversation as an original message.
	/// </summary>
	[DataContract]
	public abstract class Response : Message
	{
		/// <summary>
		/// Make a reply in the same conversation as the given message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="respondingTo"></param>
		public Response(ID nodeID, Message respondingTo, EndpointAddress nodeEndpoint) : base(nodeID, respondingTo.ConversationID, nodeEndpoint)
		{
		}
	}
}
