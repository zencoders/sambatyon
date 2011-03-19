/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 11:21 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Kademlia.Messages
{
	/// <summary>
	/// Description of FindKeyContactResponse.
	/// </summary>
	[Serializable]
	public class FindValueContactResponse : Response
	{
		private List<Contact> contacts;
		
		/// <summary>
		/// Make a new response reporting contacts to try.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="request"></param>
		/// <param name="close"></param>
		public FindValueContactResponse(ID nodeID, FindValue request, List<Contact> close) : base(nodeID, request)
		{
			contacts = close;
		}
		
		/// <summary>
		/// Return the list of contacts sent.
		/// </summary>
		/// <returns></returns>
		public List<Contact> GetContacts()
		{
			return contacts;
		}
		
		public override string GetName()
		{
			return "FIND_VALUE_RESPONSE_CONTACTS";
		}
	}
}
