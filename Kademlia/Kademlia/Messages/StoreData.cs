/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/23/2010
 * Time: 7:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Kademlia.Messages
{
	/// <summary>
	/// Send along the data in response to an affirmative StoreResponse.
	/// </summary>
	[Serializable]
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
		public StoreData(ID nodeID, StoreResponse request, ID theKey, ID theDataHash, string theData, DateTime originalPublication) : base(nodeID, request)
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
		public ID GetKey()
		{
			return key;
		}
		
		/// <summary>
		/// Get the data to store.
		/// </summary>
		/// <returns></returns>
		public string GetData()
		{
			return data;
		}
		
		/// <summary>
		/// Gets the data value hash.
		/// </summary>
		/// <returns></returns>
		public ID GetDataHash()
		{
			return dataHash;
		}
		
		/// <summary>
		/// Get when the data was originally published, in UTC.
		/// </summary>
		/// <returns></returns>
		public DateTime GetPublicationTime()
		{
			return publication.ToUniversalTime();
		}
		
		public override string GetName()
		{
			return "STORE_DATA";
		}
	}
}
