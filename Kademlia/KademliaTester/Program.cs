/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 7:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Kademlia;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel;

namespace KademliaTester
{
	/// <summary>
	/// Run some tests of the Daylight DHT.
	/// </summary>
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("ID tests");
			byte[] aData = new byte[20];
			aData[3] = 10;
			byte[] bData = new byte[20];
			bData[1] = 10;
			
			ID a = new ID(aData);
			ID b = new ID(bData);
			Debug.Assert(a != b);
			Debug.Assert(a == a);
			Debug.Assert(!(b == a));
			Debug.Assert(a < b);
			Debug.Assert(b > a);
			Debug.Assert(a.Equals(a));
			Debug.Assert(!a.Equals(b));
			Debug.Assert(a.GetHashCode() != b.GetHashCode());
			Debug.Assert(a.DifferingBit(b) == 4 * 8 - 2); // next to last bit of 4th byte differs
			Console.WriteLine("Test complete");
			
			Console.WriteLine("Testing KademilaNode");
            KademliaNode node1 = new KademliaNode(new EndpointAddress("soap.udp://localhost:8002/kademlia"));
            ServiceHost host1 = new ServiceHost(node1, new Uri("soap.udp://localhost:8002/kademlia"));
            KademliaNode node2 = new KademliaNode(new EndpointAddress("soap.udp://localhost:8001/kademlia"));
            ServiceHost host2 = new ServiceHost(node2, new Uri("soap.udp://localhost:8001/kademlia"));
//			node1.Bootstrap();
			System.Threading.Thread.Sleep(50); // Wait for the other node to process its bucket queue
			node1.JoinNetwork();
			node2.JoinNetwork();
			
			// Do a big test
			List<KademliaNode> nl = new List<KademliaNode>();
			KademliaNode lastNode = node1;
			for(int i = 0; i < 10; i++) {
				KademliaNode node = new KademliaNode();
				node.Bootstrap();
				lastNode = node;
				System.Threading.Thread.Sleep(50); // Wait for the other node to process its bucket queue
				node.JoinNetwork();
				nl.Add(node);
			}
            host1.Close();
            host2.Close();
			Console.WriteLine("Connectivity tests complete");
/*			
			// Do a store test
			ID babiesID = ID.RandomID();
			node1.Put(babiesID, "=====I eat babies=====");
			Console.WriteLine("Store tests complete");
			
			// Get it back
			IList<string> foundVals = node1.Get(babiesID);
			foreach(string s in foundVals) {
				Console.WriteLine("1 Found: " + s);
			}
			foundVals = node2.Get(babiesID);
			foreach(string s in foundVals) {
				Console.WriteLine("2 Found: " + s);
			}
			Console.WriteLine("Find tests complete");
			
			Console.WriteLine("Testing DHT");
			Dht dht = new Dht("C:\\Users\\seby\\Documents\\progetto_malgeri\\p2p-player\\nodes"+Console.ReadLine()+".xml");
			dht.Put("A", "The value for A");
			Console.WriteLine("A = " + dht.Get("A"));
			
			Console.WriteLine("Multi-value test");
			dht.Put("Animal", "cat");
			dht.Put("Animal", "dog");
			dht.Put("Animal", "double-beaver");
			dht.Put("Animal", "wombat");
			dht.Put("Animal", "alot");
			Console.WriteLine("Animal entry count " + node1.Get(ID.Hash("Animal")).Count);
			Console.WriteLine("Animal entry count " + node2.Get(ID.Hash("Animal")).Count);
			Console.WriteLine("Arbitrary animal = " + dht.Get("Animal"));
			          
*/			
			Console.WriteLine("Testing complete! Press any key to exit the program.");
            Console.ReadLine();
		}
	}
}