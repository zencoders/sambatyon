using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Description of FindKeyContactResponse.
	/// </summary>
	[DataContract]
	public class FindValueContactResponse : Response
	{
		private List<Contact> contacts;
		
		/// <summary>
		/// Make a new response reporting contacts to try.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="close"></param>
		public FindValueContactResponse(ID nodeID, FindValue request, List<Contact> close, EndpointAddress nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			contacts = close;
		}
		
		/// <summary>
		/// Return the list of contacts sent.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public List<Contact> Contacts
		{
            get { return contacts; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "FIND_VALUE_RESPONSE_CONTACTS"; }
		}
	}
}
