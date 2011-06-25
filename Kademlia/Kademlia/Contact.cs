using System;
using System.Net;
using System.ServiceModel;

namespace Kademlia
{
	/// <summary>
	/// Represents the information needed to contact another node.
	/// </summary>
	[Serializable]
	public class Contact
	{
		private ID nodeID;
        private Uri nodeEndpoint;
		
		/// <summary>
		/// Make a contact for a node with the given ID at the given location.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="endpoint"></param>
		public Contact(ID id, Uri endpoint)
		{
			nodeID = id;
			nodeEndpoint = endpoint;
		}
		
		/// <summary>
		/// Get the node's ID.
		/// </summary>
		/// <returns></returns>
		public ID NodeID {
            get { return nodeID; }
		}
		
		/// <summary>
		/// Get the node's endpoint.
		/// </summary>
		/// <returns></returns>
		public Uri NodeEndPoint {
            get { return nodeEndpoint; }
		}
		
		public override string ToString()
		{
			return NodeID.ToString() + "@" + NodeEndPoint.ToString();
		}
	}
}
