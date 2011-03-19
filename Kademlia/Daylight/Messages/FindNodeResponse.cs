/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 10:56 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Daylight.Messages
{
	/// <summary>
	/// A response to a FindNode message.
	/// Contains a list of Contacts.
	/// </summary>
	[Serializable]
	public class FindNodeResponse : Response
	{
		private List<Contact> contacts;
		
		public FindNodeResponse(ID nodeID, FindNode request, List<Contact> recommended) : base(nodeID, request)
		{
			contacts = recommended;
		}
		
		/// <summary>
		/// Gets the list of recommended contacts.
		/// </summary>
		/// <returns></returns>
		public List<Contact> GetContacts()
		{
			return contacts;
		}
		
		public override string GetName()
		{
			return "FIND_NODE_RESPONSE";
		}
	}
}
