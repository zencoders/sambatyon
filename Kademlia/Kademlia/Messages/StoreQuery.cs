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
		private ID key;
		private ID dataHash;
		private DateTime publication;
		private int valueSize;
		
		/// <summary>
		/// Make a new STORE_QUERY message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="toStore"></param>
		/// <param name="hash">A hash of the data value</param>
		/// <param name="originalPublication"></param>
		/// <param name="dataSize"></param>
		public StoreQuery(ID nodeID, ID toStore, ID hash, DateTime originalPublication, int dataSize, EndpointAddress nodeEndpoint) : base(nodeID, nodeEndpoint)
		{
			key = toStore;
			dataHash = hash;
			publication = originalPublication;
			valueSize = dataSize;
		}
		
		/// <summary>
		/// Returns the key that we want stored.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID Key
		{
            get { return key; }
            set { this.key = value; }
		}
		
		/// <summary>
		/// Gets the hash of the data value we're asking about.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public ID DataHash
		{
            get { return dataHash; }
            set { this.dataHash = value; }
		}
		
		/// <summary>
		/// Returns the size of the value we're storing, in bytes
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public int ValueSize
		{
            get { return valueSize; }
            set { this.valueSize = value; }
		}
		
		/// <summary>
		/// Get when the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
        [DataMember]
		public DateTime PublicationTime
		{
            get { return publication.ToUniversalTime(); }
            set { }
		}
		
        [DataMember]
		public override string Name
		{
            get { return "STORE_QUERY"; }
            set { }
		}
	}
}
