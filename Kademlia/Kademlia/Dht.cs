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
using System.Xml.Linq;
using System.Linq;
using System.ServiceModel;
using Persistence;
using System.Configuration;
using Metrics;
using log4net;

namespace Kademlia
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
        private static readonly ILog log = LogManager.GetLogger(typeof(Dht));
		
		private KademliaNode dhtNode;
		
		/// <summary>
		/// Create a DHT using the given master server, and specify whether to publish our IP.
		/// PRECONDITION: Create one per app or you will have a node ID collision.
		/// TODO: Fix this.
		/// </summary>
		/// <param name="overlayUrl"></param>
		/// <param name="register"></param>
		public Dht(KademliaNode dhtNode = null, bool alreadyBootstrapped = false, string btpNode = "")
		{

			// Make a new node and get port
            if (dhtNode != null)
            {
                this.dhtNode = dhtNode;
            }
            else
            {
                dhtNode = new KademliaNode();
            }
            if (!alreadyBootstrapped)
            {
                if (btpNode == "")
                {
                    int ourPort = dhtNode.GetPort();
                    log.Info("We are on UDP port " + ourPort.ToString());

                    log.Info("Getting bootstrap list...");

                    AppSettingsReader asr = new AppSettingsReader();
                    
                    XDocument xmlDoc = XDocument.Load((string)asr.GetValue("KademliaNodesFile", typeof(string)));

                    List<EndpointAddress> nodes = new List<EndpointAddress>(from node in xmlDoc.Descendants("Node")
                                select new EndpointAddress("soap.udp://" + node.Element("Host").Value + ":" + node.Element("Port").Value + "/kademlia"));

                    foreach (var node in nodes)
                    {
                        // Each line is <ip> <port>
                        if (dhtNode.AsyncBootstrap(nodes))
                        {
                            log.Debug("OK!");
                        }
                        else
                        {
                            log.Debug("Failed.");
                        }
/*                        try
                        {
                            log.Debug("Bootstrapping with " + node.Host + ":" + node.Port);
                            EndpointAddress bootstrapNode = new EndpointAddress("soap.udp://" + node.Host + ":" + node.Port + "/kademlia");
                            if (dhtNode.Bootstrap(bootstrapNode))
                            {
                                log.Debug("OK!");
                            }
                            else
                            {
                                log.Debug("Failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Bad entry!", ex);
                        }*/
                    }
                }
                else
                {
                    // Each line is <ip> <port>
                    try
                    {
                        log.Debug("Bootstrapping with " + btpNode);
                        EndpointAddress bootstrapNode = new EndpointAddress(btpNode);
                        if (dhtNode.Bootstrap(bootstrapNode))
                        {
                            log.Debug("OK!");
                        }
                        else
                        {
                            log.Debug("Failed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Bad entry!", ex);
                    }
                }
            }
            else
            {
                log.Info("Self Bootstrapping");
                dhtNode.Bootstrap();
            }
			// Join the network officially
			log.Info("Trying to join network....");
			if(dhtNode.JoinNetwork()) {
				log.Info("Online");
			} else {
				log.Warn("Unable to connect to Kademlia overlay!\n"
				                   + "Check that nodes list has accessible nodes.");
			}
		}
		
		/// <summary>
		/// Retrieve a value from the DHT.
		/// </summary>
		/// <param name="key">The key of the value to retrieve.</param>
		/// <returns>an arbitrary value stored for the key, or null if no values are found</returns>
		public KademliaResource Get(string key)
		{
            IList<KademliaResource> found = dhtNode.Get(key);
			if(found.Count > 0) {
                IList<KademliaResource> ordered = found.AsParallel().OrderByDescending(
                    d => QualityCalculator.calculateQualityCoefficient(
                        key, new string[3] { d.Tag.Album, d.Tag.Artist, d.Tag.Title },
                        0, 0, d.Tag.Bitrate, d.Tag.Channels, d.Tag.SampleRate)
                        ).ToList();
				return ordered[0]; // An arbitrary value
			} else {
				return null; // Nothing there
			}
		}
		
		/// <summary>
		/// Retrieve all applicable values from the DHT.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<KademliaResource> GetAll(string key)
		{
            IList<KademliaResource> found = dhtNode.Get(key);
            if (found.Count > 0)
            {
                return found.AsParallel().OrderByDescending(
                    d => QualityCalculator.calculateQualityCoefficient(
                        key, new string[3] { d.Tag.Album, d.Tag.Artist, d.Tag.Title },
                        0, 0, d.Tag.Bitrate, d.Tag.Channels, d.Tag.SampleRate)
                        ).ToList();
            }
            else
            {
                return found;
            }
		}
		
		/// <summary>
		/// Puts a value in the DHT under a key.
		/// </summary>
		/// <param name="key">Can be any length, is hashed internally.</param>
		/// <param name="val">Can be up to and including MaxSize() UTF-8 characters.</param>
		public void Put(string filename)
		{
			dhtNode.Put(filename);
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
