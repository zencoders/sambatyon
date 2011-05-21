/*
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
            KademliaNode node1 = new KademliaNode(new EndpointAddress("net.tcp://localhost:8002/kademlia"));
            ServiceHost host1 = new ServiceHost(node1, new Uri("net.tcp://localhost:8002/kademlia"));
            KademliaNode node2 = new KademliaNode(new EndpointAddress("net.tcp://localhost:8001/kademlia"));
            ServiceHost host2 = new ServiceHost(node2, new Uri("net.tcp://localhost:8001/kademlia"));
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