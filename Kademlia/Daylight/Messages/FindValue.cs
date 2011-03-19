/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 11:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Daylight.Messages
{
	/// <summary>
	/// Represents a request to get a value.
	/// Reciever should either send key or a node list.
	/// </summary>
	[Serializable]
	public class FindValue : Message
	{
		private ID key;
		
		/// <summary>
		/// Make a new FindValue message.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="wantedKey"></param>
		public FindValue(ID nodeID, ID wantedKey) : base(nodeID)
		{
			this.key = wantedKey;
		}
		
		/// <summary>
		/// Return the key this message wants.
		/// </summary>
		/// <returns></returns>
		public ID GetKey() {
			return key;
		}
		
		public override string GetName()
		{
			return "FIND_VALUE";
		}
	}
}
