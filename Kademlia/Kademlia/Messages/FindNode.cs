/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 10:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Kademlia.Messages
{
	/// <summary>
	/// A message used to search for a node.
	/// </summary>
	[Serializable]
	public class FindNode : Message
	{
		private ID target;
		
		/// <summary>
		/// Make a new FIND_NODE message
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="toFind"></param>
		public FindNode(ID nodeID, ID toFind) : base(nodeID)
		{
			target = toFind;
		}
		
		/// <summary>
		/// Get the target of this message.
		/// </summary>
		/// <returns></returns>
		public ID GetTarget()
		{
			return target;
		}
		
		public override string GetName() 
		{
			return "FIND_NODE";
		}
	}
}
