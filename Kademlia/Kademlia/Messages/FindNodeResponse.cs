using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// A response to a FindNode message.
	/// Contains a list of Contacts.
	/// </summary>
	[DataContract]
	public class FindNodeResponse : Response
	{
		private List<Contact> contacts;
		
		public FindNodeResponse(ID nodeID, FindNode request, List<Contact> recommended, EndpointAddress nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			contacts = recommended;
		}
		
		/// <summary>
		/// Gets the list of recommended contacts.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public List<Contact> Contacts
		{
            get {return contacts;}
            set { this.contacts = value; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "FIND_NODE_RESPONSE"; }
            set { }
		}
	}
}
