/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 7:18 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;

namespace Kademlia
{
	/// <summary>
	/// Represents the information needed to contact another node.
	/// </summary>
	[Serializable]
	public class Contact
	{
		private ID nodeID;
		private IPEndPoint nodeEndpoint;
		
		/// <summary>
		/// Make a contact for a node with the given ID at the given location.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="endpoint"></param>
		public Contact(ID id, IPEndPoint endpoint)
		{
			nodeID = id;
			nodeEndpoint = endpoint;
		}
		
		/// <summary>
		/// Get the node's ID.
		/// </summary>
		/// <returns></returns>
		public ID GetID() {
			return nodeID;
		}
		
		/// <summary>
		/// Get the node's endpoint.
		/// </summary>
		/// <returns></returns>
		public IPEndPoint GetEndPoint() {
			return nodeEndpoint;
		}
		
		public override string ToString()
		{
			return GetID().ToString() + "@" + GetEndPoint().ToString();
		}
	}
}
