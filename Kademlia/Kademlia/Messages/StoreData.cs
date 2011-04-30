using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// Send along the data in response to an affirmative StoreResponse.
	/// </summary>
	[DataContract]
	public class StoreData : Response
	{
		private ID key;
		private ID dataHash; // Distinguish multiple values for a given key
		private string data;
		private DateTime publication;
		
		/// <summary>
		/// Make a mesage to store the given data.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="theKey"></param>
		/// <param name="theDataHash"></param>
		/// <param name="theData"></param>
		/// <param name="originalPublication"></param>
		public StoreData(ID nodeID, StoreResponse request, ID theKey, ID theDataHash, string theData, DateTime originalPublication, EndpointAddress nodeEndpoint) : base(nodeID, request, nodeEndpoint)
		{
			key = theKey;
			data = theData;
			dataHash = theDataHash;
			publication = originalPublication;
		}
		
		/// <summary>
		/// Return the key we want to store at.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID Key
		{
            get { return key; }
		}
		
		/// <summary>
		/// Get the data to store.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public string Data
		{
            get { return data; }
		}
		
		/// <summary>
		/// Gets the data value hash.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID DataHash
		{
            get { return dataHash; }
		}
		
		/// <summary>
		/// Get when the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public DateTime PublicationTime
		{
            get { return publication.ToUniversalTime(); }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "STORE_DATA"; }
		}
	}
}
