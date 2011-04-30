using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Represents a request to get a value.
	/// Reciever should either send key or a node list.
	/// </summary>
	[DataContract]
	public class FindValue : Message
	{
		private ID key;
		
		/// <summary>
		/// Make a new FindValue message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="wantedKey"></param>
		public FindValue(ID nodeID, ID wantedKey, EndpointAddress nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			this.key = wantedKey;
		}
		
		/// <summary>
		/// Return the key this message wants.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID Key {
            get { return key; }
            set { this.Key = value; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE"; }
            set { }
		}
	}
}
