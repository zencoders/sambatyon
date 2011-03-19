/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 9:37 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.Serialization;

namespace Daylight.Messages
{
	/// <summary>
	/// Represents a generic DHT RPC message
	/// </summary>
	[Serializable]
	public abstract class Message
	{
		// All messages include sender id
		private ID senderID;
		private ID conversationID;
		
		/// <summary>
		/// Make a new message, recording the sender's ID.
		/// </summary>
		/// <param name="senderID"></param>
		public Message(ID senderID) {
			this.senderID = senderID;
			conversationID = ID.RandomID();
		}
		
		/// <summary>
		/// Make a new message in a given conversation.
		/// </summary>
		/// <param name="senderID"></param>
		/// <param name="conversationID"></param>
		public Message(ID senderID, ID conversationID) {
			this.senderID = senderID;
			this.conversationID = conversationID;
		}
		
		/// <summary>
		/// Get the name of the message.
		/// </summary>
		/// <returns></returns>
		public abstract string GetName();
		
		/// <summary>
		/// Get the ID of the sender of the message.
		/// </summary>
		/// <returns></returns>
		public ID GetSenderID() {
			return senderID;
		}
		
		/// <summary>
		/// Gets the ID of this conversation.
		/// </summary>
		/// <returns></returns>
		public ID GetConversationID() {
			return conversationID;
		}
	}
}
