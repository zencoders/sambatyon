using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia.Messages
{
	/// <summary>
	/// A message asking if another node will store data for us, and if we need to send the data.
	/// Maybe they already have it.
	/// We have to send the timestamp with it for people to republish stuff.
	/// </summary>
	[DataContract]
	public class StoreQuery : Message
	{
		private ID tagHash;
		private DateTime publication;
		
		/// <summary>
		/// Make a new STORE_QUERY message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="toStore"></param>
		/// <param name="hash">A hash of the data value</param>
		/// <param name="originalPublication"></param>
		/// <param name="dataSize"></param>
		public StoreQuery(ID nodeID, ID hash, DateTime originalPublication, Uri nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			tagHash = hash;
			publication = originalPublication;
		}
		
		/// <summary>
		/// Gets the hash of the data value we're asking about.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID TagHash
		{
            get { return tagHash; }
            set { this.tagHash = value; }
		}

		/// <summary>
		/// Get when the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public DateTime PublicationTime
		{
            get { return publication.ToUniversalTime(); }
            set { this.publication = value; }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "STORE_QUERY"; }
            set { }
		}
	}
}
