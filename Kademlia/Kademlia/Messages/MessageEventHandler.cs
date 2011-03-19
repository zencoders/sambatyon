/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 10:23 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Kademlia.Messages
{
	/// <summary>
	/// A delegate for handling message events.
	/// </summary>
	public delegate void MessageEventHandler<T>(Contact sender, T message) where T : Message;
}
