using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// A reply to a store query.
	/// Say if we're willing to store the data, and if we already have it.
	/// </summary>
	[DataContract]
	public class StoreResponse : Response
	{
		private bool sendData;
		
		public StoreResponse(ID nodeID, StoreQuery query, bool accept, EndpointAddress nodeEndpoint) : base(nodeID, query, nodeEndpoint)
		{
			sendData = accept;
		}
		
		/// <summary>
		/// Returns true if we should send them the data.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public bool ShouldSendData
		{
            get { return sendData; }
            set { this.sendData = value; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "STORE_RESPONSE"; }
            set { }
		}
	}
}
