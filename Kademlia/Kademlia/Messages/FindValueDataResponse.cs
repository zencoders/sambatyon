using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using Persistence;

namespace Kademlia.Messages
{
	/// <summary>
	/// Send data in reply to a FindVAlue
	/// </summary>
	[DataContract]
	public class FindValueDataResponse : Response
	{
		private IList<KademliaResource> vals;
		
		/// <summary>
		/// Make a new response.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="data"></param>
		public FindValueDataResponse(ID nodeID, FindValue request, IList<KademliaResource> data, EndpointAddress nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			vals = data;
		}
		
		/// <summary>
		/// Get the values returned for the key
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public IList<KademliaResource> Values
		{
            get { return vals; }
            set { this.vals = value; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE_RESPONSE_DATA"; }
            set { }
		}
	}
}
