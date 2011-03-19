/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/23/2010
 * Time: 6:49 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Daylight.Messages
{
	/// <summary>
	/// A message asking if another node will store data for us, and if we need to send the data.
	/// Maybe they already have it.
	/// We have to send the timestamp with it for people to republish stuff.
	/// </summary>
	[Serializable]
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
		public StoreQuery(ID nodeID, ID toStore, ID hash, DateTime originalPublication, int dataSize) : base(nodeID)
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
		public ID GetKey()
		{
			return key;
		}
		
		/// <summary>
		/// Gets the hash of the data value we're asking about.
		/// </summary>
		/// <returns></returns>
		public ID GetDataHash()
		{
			return dataHash;
		}
		
		/// <summary>
		/// Returns the size of the value we're storing, in bytes
		/// </summary>
		/// <returns></returns>
		public int GetValueSize()
		{
			return valueSize;
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
			return "STORE_QUERY";
		}
	}
}
