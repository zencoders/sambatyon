/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/23/2010
 * Time: 6:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Kademlia.Messages
{
	/// <summary>
	/// A reply to a store query.
	/// Say if we're willing to store the data, and if we already have it.
	/// </summary>
	[Serializable]
	public class StoreResponse : Response
	{
		bool sendData;
		
		public StoreResponse(ID nodeID, StoreQuery query, bool accept) : base(nodeID, query)
		{
			sendData = accept;
		}
		
		/// <summary>
		/// Returns true if we should send them the data.
		/// </summary>
		/// <returns></returns>
		public bool ShouldSendData()
		{
			return sendData;
		}
		
		public override string GetName()
		{
			return "STORE_RESPONSE";
		}
	}
}
