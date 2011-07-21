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
		/// </summary>
        /// <param name="dhtNode">The KademliaNode that is used to communicate using the protocol</param>
        /// <param name="alreadyBootstrapped">Checks if the node have or not to bootstrap</param>
        /// <param name="btpNode">The node to bootstrap with (can be leaved null)</param>
		public Dht(KademliaNode dhtNode = null, bool alreadyBootstrapped = false, string btpNode = "")
		{
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
                        if (dhtNode.AsyncBootstrap(nodes))
                        {
                            log.Debug("OK!");
                        }
                        else
                        {
                            log.Debug("Failed.");
                        }
                    }
                }
                else
                {
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
		/// <returns>the best semantically matching value stored for the key, or null if no values are found</returns>
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
        /// <param name="key">The key of the value to retrieve.</param>
		/// <returns>All the list of resources found on network, ordered by semantic affinity</returns>
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
        /// <param name="filename">The filename of resource to store into the network</param>
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
