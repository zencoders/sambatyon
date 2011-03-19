/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 11:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Daylight.Messages
{
	/// <summary>
	/// Represents a response message, in the same conversation as an original message.
	/// </summary>
	[Serializable]
	public abstract class Response : Message
	{
		/// <summary>
		/// Make a reply in the same conversation as the given message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="respondingTo"></param>
		public Response(ID nodeID, Message respondingTo) : base(nodeID, respondingTo.GetConversationID())
		{
		}
	}
}
