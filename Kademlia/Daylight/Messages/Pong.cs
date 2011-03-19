/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 9:46 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Daylight.Messages
{
	/// <summary>
	/// Represents a ping reply.
	/// </summary>
	[Serializable]
	public class Pong : Response
	{
		public Pong(ID senderID, Ping ping) : base(senderID, ping)
		{
		}
		
		public override string GetName()
		{
			return "PONG";
		}
	}
}
