using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// A message used to search for a node.
	/// </summary>
	[DataContract]
	public class FindNode : Message
	{
		private ID target;
		
		/// <summary>
		/// Make a new FIND_NODE message
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="toFind"></param>
		public FindNode(ID nodeID, ID toFind, EndpointAddress nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			target = toFind;
		}
		
		/// <summary>
		/// Get the target of this message.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID Target
		{
            get { return target; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "FIND_NODE"; }
		}
	}
}
