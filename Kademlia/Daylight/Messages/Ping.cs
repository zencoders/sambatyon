/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 9:44 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Daylight.Messages
{
	/// <summary>
	/// Represents a ping message, used to see if a remote node is up.
	/// </summary>
	[Serializable]
	public class Ping : Message
	{
		public Ping(ID senderID) : base(senderID)
		{
		}
		
		public override string GetName()
		{
			return "PING";
		}
	}
}
