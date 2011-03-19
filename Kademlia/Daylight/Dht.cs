/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 7:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Collections.Generic;

namespace Daylight
{
	/// <summary>
	/// This is the class you use to use the library.
	/// You can put and get values.
	/// It is responsible for bootstraping the local node and connecting to the appropriate overlay.
	/// It also registers us with the overlay.
	/// </summary>
	public class Dht
	{
		private const int MAX_SIZE = 8 * 1024; // 8K is big
		private const string DEFAULT_OVERLAY_URL = "http://comsmart.org/Daylight/";
		private const string LIST_FRAGMENT = "list.php";
		private const string REGISTER_FRAGMENT = "ping.php?port="; // Append port number.
		
		private KademliaNode dhtNode;
		
		/// <summary>
		/// Create a new DHT. It should connect to the default overlay network
		/// if possible, or use an existing connection, and do default things
		/// with regard to UPnP and storage on the local filesystem. It also
		/// will by default announce itself to the master list.
		/// </summary>
		public Dht() : this(DEFAULT_OVERLAY_URL, true)
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Create a DHT using the given master server, and specify whether to publish our IP.
		/// PRECONDITION: Create one per app or you will have a node ID collision.
		/// TODO: Fix this.
		/// </summary>
		/// <param name="overlayUrl"></param>
		/// <param name="register"></param>
		public Dht(string overlayUrl, bool register)
		{
			// Make a new node and get port
			dhtNode = new KademliaNode();
			int ourPort = dhtNode.GetPort();
			Console.WriteLine("We are on UDP port " + ourPort.ToString());
			
			// Bootstrap with some nodes
			WebClient downloader = new WebClient();
			downloader.Proxy = null; // TODO: Let client specify proxy
			Console.WriteLine("Getting bootstrap list...");
			// TODO: Handle 404, etc.
			string list = downloader.DownloadString(overlayUrl + LIST_FRAGMENT); // Get master list
			string[] hosts = list.Split('\n');
			
			foreach(string host in hosts) {
				// Each line is <ip> <port>
				string[] parts = host.Split(' ');
				if(parts.Length == 2) {
					try {
						IPEndPoint bootstrapNode = new IPEndPoint(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
						Console.Write("Bootstrapping with " + bootstrapNode.ToString() + ": ");
						if(dhtNode.Bootstrap(bootstrapNode)) {
							Console.WriteLine("OK!");
						} else {
							Console.WriteLine("Failed.");
						}
					} catch (Exception ex) {
						Console.WriteLine("Bad entry!");
					}
				}
			}
			
			// Join the network officially
			Console.WriteLine("Joining network...");
			if(dhtNode.JoinNetwork()) {
				Console.WriteLine("Daylight online");
				if(register) { // Announce our presence
					downloader.DownloadString(overlayUrl + REGISTER_FRAGMENT + ourPort.ToString());
					Console.WriteLine("Announced presence");
				}
				
			} else {
				Console.WriteLine("Unable to connect to Daylight overlay!\n"
				                   + "Check that the master server is returning accessible nodes.");
			}
		}
		
		/// <summary>
		/// Retrieve a value from the DHT.
		/// </summary>
		/// <param name="key">The key of the value to retrieve.</param>
		/// <returns>an arbitrary value stored for the key, or null if no values are found</returns>
		public string Get(string key)
		{
			IList<string> found = dhtNode.Get(ID.Hash(key));
			if(found.Count > 0) {
				return found[0]; // An arbitrary value
			} else {
				return null; // Nothing there
			}
		}
		
		/// <summary>
		/// Retrieve all applicable values from the DHT.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<string> GetAll(string key)
		{
			return dhtNode.Get(ID.Hash(key));
		}
		
		/// <summary>
		/// Puts a value in the DHT under a key.
		/// </summary>
		/// <param name="key">Can be any length, is hashed internally.</param>
		/// <param name="val">Can be up to and including MaxSize() UTF-8 characters.</param>
		public void Put(string key, string val)
		{
			dhtNode.Put(ID.Hash(key), val);
		}
		
		/// <summary>
		/// Returns the maximum size of individual puts.
		/// </summary>
		/// <returns>the maximum size of individual puts</returns>
		public int MaxSize()
		{
			return MAX_SIZE;
		}
	}
}
