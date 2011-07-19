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

﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using Kademlia.Messages;
using UdpTransportBinding;
using Persistence.Tag;
using Persistence;
using System.Configuration;
using log4net;

namespace Kademlia
{
	/// <summary>
	/// Functions as a peer in the overlay network.
	/// </summary>
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class KademliaNode : IKademliaNode
	{
		// Identity
		private ID nodeID;
        private EndpointAddress nodeEndpoint;
        private EndpointAddress transportEndpoint;
		
		// Network State
		private BucketList contactCache;
		private Thread bucketMinder; // Handle updates to cache
		private const int CHECK_INTERVAL = 100;
		private List<Contact> contactQueue; // Add contacts here to be considered for caching
		private const int MAX_QUEUE_LENGTH = 10;
        private const string DEFAULT_TRANSPORT_ENDPOINT = "soap.udp://localhost:9997/transport_protocol/";
        private const string DEFAULT_ENDPOINT = "soap.udp://localhost:10000/kademlia";
        private const string DEFAULT_REPOSITORY = @"..\..\Resource\KademliaDatabase";
		
		// Response cache
		// We want to be able to synchronously wait for responses, so we have other threads put them in this cache.
		// We also need to discard old ones.
		private struct CachedResponse {
			public Response response;
			public DateTime arrived;
		}
		private SortedList<ID, CachedResponse> responseCache;
        private AutoResetEvent responseCacheLocker;
        private long max_time = 5000000;
        private TimeSpan MAX_SYNC_WAIT = new TimeSpan(5000000); // 500 ms in ticks
		
		// Application (datastore)
        private KademliaRepository datastore; 
		private SortedList<ID, DateTime> acceptedStoreRequests; // Store a list of what put requests we actually accepted while waiting for data.
		// The list of put requests we sent is more complex
		// We need to keep the data and timestamp, but don't want to insert it in our storage.
		// So we keep it in a cache, and discard it if it gets too old.
		private struct OutstandingStoreRequest {
			public ID key;
			public CompleteTag val;
			public DateTime publication;
			public DateTime sent;
		}
		private SortedList<ID, OutstandingStoreRequest> sentStoreRequests;
		
		// We need a thread to go through and expire all these things
		private Thread authMinder;
		private TimeSpan MAX_CACHE_TIME = new TimeSpan(0, 0, 30);
		
		// How much clock skew do we tolerate?
		private TimeSpan MAX_CLOCK_SKEW = new TimeSpan(1, 0, 0);
		
		// Kademlia config
		private const int PARALELLISM = 3; // Number of requests to run at once for iterative operations.
		private const int NODES_TO_FIND = 20; // = k = bucket size
		private static TimeSpan EXPIRE_TIME = new TimeSpan(24, 0, 0); // Time since original publication to expire a value
		private static TimeSpan REFRESH_TIME = new TimeSpan(1, 0, 0); // Time since last bucket access to refresh a bucket
		private static TimeSpan REPLICATE_TIME = new TimeSpan(1, 0, 0); // Interval at which we should re-insert our whole database
		private DateTime lastReplication;
		private static TimeSpan REPUBLISH_TIME = new TimeSpan(23, 0, 0); // Interval at which we should re-insert our values with new publication times
		
		// How often do we run high-level maintainance (expiration, etc.)
		private static TimeSpan MAINTAINANCE_INTERVAL = new TimeSpan(0, 10, 0);
		private Thread maintainanceMinder;

        private static readonly ILog log = LogManager.GetLogger(typeof(KademliaNode));
		
		#region Setup	
		
		/// <summary>
		/// Make a node on a random available port, using an ID specific to this machine.
		/// </summary>
		public KademliaNode() : this(new EndpointAddress(DEFAULT_ENDPOINT), ID.HostID(), new EndpointAddress(DEFAULT_TRANSPORT_ENDPOINT))
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Make a node with a specified ID.
		/// </summary>
		/// <param name="id"></param>
		public KademliaNode(ID id) : this(new EndpointAddress(DEFAULT_ENDPOINT), id, new EndpointAddress(DEFAULT_TRANSPORT_ENDPOINT))
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Make a node on a specified port.
		/// </summary>
		/// <param name="port"></param>
		public KademliaNode(EndpointAddress addr) : this(addr, ID.HostID(), new EndpointAddress(DEFAULT_TRANSPORT_ENDPOINT))
		{
			// Nothing to do!
		}

        public KademliaNode(EndpointAddress addr, EndpointAddress transportAddr)
            : this(addr, ID.HostID(), transportAddr)
        {
            // Nothing to do!
        }
		
		/// <summary>
		/// Make a node on a specific port, with a specified ID
		/// </summary>
		/// <param name="port"></param>
		public KademliaNode(EndpointAddress addr, ID id, EndpointAddress transportAddr)
		{
			// Set up all our data
            AppSettingsReader asr = new AppSettingsReader();
            nodeEndpoint = addr;
            transportEndpoint = transportAddr;
			nodeID = id;
			contactCache = new BucketList(nodeID);
			contactQueue = new List<Contact>();
            RepositoryConfiguration conf = new RepositoryConfiguration(new { data_dir = (string) asr.GetValue("KademliaRepository", typeof(string)) });
            datastore = new KademliaRepository("Raven", conf);
			acceptedStoreRequests = new SortedList<ID, DateTime>();
			sentStoreRequests = new SortedList<ID, KademliaNode.OutstandingStoreRequest>();
			responseCache = new SortedList<ID, KademliaNode.CachedResponse>();
            responseCacheLocker = new AutoResetEvent(true);
			lastReplication = default(DateTime);

			// Start minding the buckets
			bucketMinder = new Thread(new ThreadStart(MindBuckets));
			bucketMinder.IsBackground = true;
			bucketMinder.Start();
			
			// Start minding the conversation state caches
			authMinder = new Thread(new ThreadStart(MindCaches));
			authMinder.IsBackground = true;
			authMinder.Start();

			// Start maintainance
			maintainanceMinder = new Thread(new ThreadStart(MindMaintainance));
			maintainanceMinder.IsBackground = true;
			maintainanceMinder.Start();
		}
		
		/// <summary>
		/// Bootstrap by pinging a loacl node on the specified port.
		/// </summary>
		/// <param name="loaclPort"></param>
		/// <returns></returns>
		public bool Bootstrap()
		{
			return Bootstrap(nodeEndpoint);
		}
			
		/// <summary>
		/// Bootstrap the node by having it ping another node. Returns true if we get a response.
		/// </summary>
		/// <param name="other"></param>
		public bool Bootstrap(EndpointAddress other)
		{
			// Send a blocking ping.
			bool worked = SyncPing(other);
			Thread.Sleep(CHECK_INTERVAL); // Wait for them to notice us
			return worked;
		}

        public bool AsyncBootstrap(IList<EndpointAddress> others)
        {
            Dictionary<ID, bool> conversationIds = new Dictionary<ID, bool>();
            foreach (EndpointAddress other in others)
            {
                asyncPing(other, ref conversationIds);
            }

            DateTime called = DateTime.Now;
            while (DateTime.Now < called.Add(new TimeSpan(max_time * conversationIds.Count)))
            {
                // If we got a response, send it up
                findPingResponseCache(ref conversationIds);
                Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
            }

            foreach (ID id in conversationIds.Keys)
            {
                if (conversationIds[id])
                {
                    return true;
                }
            }
            return false;
        }
		
		/// <summary>
		/// Join the network.
		/// Assuming we have some contacts in our cache, get more by IterativeFindNoding ourselves.
		/// Then, refresh most (TODO: all) buckets.
		/// Returns true if we are connected after all that, false otherwise.
		/// </summary>
		public bool JoinNetwork() {
			log.Info("Joining network");
			IList<Contact> found = IterativeFindNode(nodeID);
			if(found == null) {
				log.Info("Found <null list>");
			} else {
				foreach(Contact c in found) {
					log.Info("Found contact: " + c.ToString());
				}
			}			
			// Should get very nearly all of them
			// RefreshBuckets(); // Put this off until first maintainance.
			if(contactCache.GetCount() > 0) {
				log.Info("Joined");
				return true;
			} else {
				log.Info("Failed to join! No other nodes known!");
				return false;
			}
		}
		#endregion
		
		#region Interface
		
		/// <summary>
		/// Returns the ID of the node
		/// </summary>
		/// <returns></returns>
		public ID GetID() 
		{
			return nodeID;
		}
		
		/// <summary>
		/// Return the port we listen on.
		/// </summary>
		/// <returns></returns>
		public int GetPort()
		{
            return nodeEndpoint.Uri.Port;
		}
		
		/// <summary>
		/// Store something in the DHT as the original publisher.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void Put(string filename)
		{
            CompleteTag fileTag = new CompleteTag(filename);
            datastore.StoreResource(fileTag, this.transportEndpoint.Uri, DateTime.Now);
			IterativeStore(fileTag, DateTime.Now);
			// TODO: republish
		}
		
		/// <summary>
		/// Gets values for a key from the DHT. Returns the values or an empty list.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<KademliaResource> Get(string key)
		{
            KademliaResource[] results = datastore.SearchFor(key);
            IList<KademliaResource> found;
            if (results.Length > 0)
            {
                found = new List<KademliaResource>(results);
            }
            else
            {
                found = new List<KademliaResource>();
            }
            IList<Contact> close;
            IterativeFindValue(key, ref found, out close);
            if (found == null)
            { // Empty list for nothing found
                return new List<KademliaResource>();
            }
            else
            {
                Dictionary<string, KademliaResource> toRet = new Dictionary<string,KademliaResource>();
                Console.WriteLine(found.Count);

                // DEBUG PRINTS
                foreach (KademliaResource kr in found)
                {
                    Console.WriteLine(" ^^^^^ Found Resources " + kr.Id);
                    foreach (DhtElement u in kr.Urls)
                    {
                        Console.WriteLine(u.Url);
                    }
                    Console.WriteLine("^^^^^");
                }
                //END

                foreach(KademliaResource el in found)
                {
                    if (toRet.ContainsKey(el.Tag.FileHash))
                    {
                        toRet[el.Tag.FileHash].MergeTo(el);
                    }
                    else
                    {
                        toRet[el.Tag.FileHash] = el;
                    }
                }

                // DEBUG PRINTS
                foreach(KademliaResource kr in toRet.Values)
                {
                    Console.WriteLine(" ^^^^^ Resource " + kr.Id);
                    foreach (DhtElement u in kr.Urls)
                    {
                        Console.WriteLine(u.Url);
                    }
                    Console.WriteLine("^^^^^");
                }
                // END

                return new List<KademliaResource>(toRet.Values);
            }
		}
		#endregion
		
		#region Maintainance Operations
		/// <summary>
		/// Expire old data, replicate all data, refresh needy buckets.
		/// </summary>
		private void MindMaintainance()
		{
            log.Info("Launched Maintenance thread!");
			while(true) {
				Thread.Sleep(MAINTAINANCE_INTERVAL);
				log.Info("Performing maintainance");
				// Expire old
				datastore.Expire();
				//Log(datastore.GetKeys().Count + " keys stored.");
				
				// Replicate all if needed
				// We get our own lists to iterate
				if(DateTime.Now > lastReplication.Add(REPLICATE_TIME)) {
					log.Debug("Replicating data");
                    foreach (KademliaResource kr in datastore.GetAllElements())
                    {
                        foreach (Persistence.DhtElement dhtEl in kr.Urls)
                        {
                            try
                            {
                                IterativeStore(kr.Tag, (DateTime)dhtEl.Publication, new EndpointAddress(dhtEl.Url));
                            }
                            catch (Exception ex)
                            {
                                log.Error("Could not replicate", ex);
                            }
                        }
                    }
					lastReplication = DateTime.Now;
				}
				
				// Refresh any needy buckets
				RefreshBuckets();
				log.Info("Done Replication");
			}
		}
		
		/// <summary>
		/// Look for nodes to go in buckets we haven't touched in a while.
		/// </summary>
		private void RefreshBuckets()
		{
			log.Info("Refreshing buckets");
			IList<ID> toLookup = contactCache.IDsForRefresh(REFRESH_TIME);
			foreach(ID key in toLookup) {
				IterativeFindNode(key);
			}
			log.Info("Refreshed buckets");
		}
		
		#endregion
		
		#region Iterative Operations

        private void IterativeStore(CompleteTag tag, DateTime originalInsertion, EndpointAddress endpoint = null)
        {
            IList<Contact> closest = IterativeFindNode(ID.FromString(tag.TagHash));
            log.Info("Storing at " + closest.Count + " nodes");
            foreach (Contact c in closest)
            {
                // Store a copy at each
                if (endpoint != null)
                {
                    Console.WriteLine("Using passed endpoint (" + endpoint + ") for Sync Store");
                    SyncStore(c, tag, originalInsertion, endpoint);
                }
                else
                {
                    Console.WriteLine("Using internal endpoint (" + nodeEndpoint + ") for Sync Store");
                    SyncStore(c, tag, originalInsertion, nodeEndpoint);
                }
            }
        }

		/// <summary>
		/// Do an iterativeFindNode operation.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		private IList<Contact> IterativeFindNode(ID target)
		{
            // Log the lookup
            if (target != nodeID)
            {
                contactCache.Touch(target);
            }

            // Get the alpha closest nodes to the target
            // TODO: Should actually pick from a certain bucket.
            SortedList<ID, Contact> shortlist = new SortedList<ID, Contact>();
            foreach (Contact c in contactCache.CloseContacts(PARALELLISM, target, null))
            {
                log.Info("Adding contact " + c.NodeEndPoint + " to shortlist");
                shortlist.Add(c.NodeID, c);
            }

            int shortlistIndex = 0; // Everyone before this is up.

            // Make an initial guess for the closest node
            Contact closest = null;
            foreach (Contact toAsk in shortlist.Values)
            {
                if (closest == null || (toAsk.NodeID ^ target) < (closest.NodeID ^ target))
                {
                    closest = toAsk;
                }
            }

            // Until we run out of people to ask or we're done...
            while (shortlistIndex < shortlist.Count && shortlistIndex < NODES_TO_FIND)
            {
                // Try the first alpha unexamined contacts
                bool foundCloser = false; // TODO: WTF does the spec want with this
                Dictionary<ID, bool> conversationIds = new Dictionary<ID,bool>();
                for (int i = shortlistIndex; i < shortlistIndex + PARALELLISM && i < shortlist.Count; i++)
                {
                    asyncFindNode(shortlist.Values[i], target, ref conversationIds);
                }

                List<Contact> suggested = new List<Contact>();

                DateTime called = DateTime.Now;
                while (DateTime.Now < called.Add(new TimeSpan(max_time*conversationIds.Count)))
                {
                    // If we got a response, send it up
                    //FindNodeResponse resp = GetCachedResponse<FindNodeResponse>(question.ConversationID);
                    findNodeResponseCache(ref conversationIds, ref suggested);
                    Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
                }

                int y = 0;
                foreach(ID id in conversationIds.Keys)
                {
                    if (! conversationIds[id])
                    {
                        // Node down. Remove from shortlist and adjust loop indicies
                        log.Info("Node is down. Removing it from shortlist!");
                        shortlist.RemoveAt(y);
                        shortlistIndex--;
                        y--;
                    }
                    y++;
                }
                // Add suggestions to shortlist and check for closest
                foreach (Contact suggestion in suggested)
                {
                    if (!shortlist.ContainsKey(suggestion.NodeID))
                    { // Contacts aren't value types so we have to do this.
                        shortlist.Add(suggestion.NodeID, suggestion);
                    }
                    if ((suggestion.NodeID ^ target) < (closest.NodeID ^ target))
                    {
                        closest = suggestion;
                        foundCloser = true;
                    }
                }

                shortlistIndex += PARALELLISM;
            }

            // Drop extra ones
            // TODO: This isn't what the protocol says at all.
            while (shortlist.Count > NODES_TO_FIND)
            {
                shortlist.RemoveAt(shortlist.Count - 1);
            }

            return shortlist.Values;
		}

		/// <summary>
		/// Perform a Kademlia iterativeFind* operation.
		/// If getValue is true, it sends out a list of strings if values are found, or null none are.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="getValue">true for FindValue, false for FindNode</param>
		/// <param name="vals"></param>
		/// <returns></returns>
		private IList<Contact> IterativeFindValue(string target, ref IList<KademliaResource> vals)
		{
			// Log the lookup
			if(ID.Hash(target) != nodeID) {
				contactCache.Touch(ID.Hash(target));
			}
			
			// Get the alpha closest nodes to the target
			// TODO: Should actually pick from a certain bucket.
			SortedList<ID, Contact> shortlist = new SortedList<ID, Contact>();
			foreach(Contact c in contactCache.CloseContacts(PARALELLISM, ID.Hash(target), null)) {
				shortlist.Add(c.NodeID, c);
			}
			
			int shortlistIndex = 0; // Everyone before this is up.
			
			// Make an initial guess for the closest node
			Contact closest = null;
			foreach(Contact toAsk in shortlist.Values) {
                if (closest == null || (toAsk.NodeID ^ ID.Hash(target)) < (closest.NodeID ^ ID.Hash(target)))
                {
					closest = toAsk;
				}
			}
			
			// Until we run out of people to ask or we're done...
			while(shortlistIndex < shortlist.Count && shortlistIndex < NODES_TO_FIND) {
				// Try the first alpha unexamined contacts
				bool foundCloser = false; // TODO: WTF does the spec want with this
                Dictionary<ID, bool> conversationIds = new Dictionary<ID,bool>();
				for(int i = shortlistIndex; i < shortlistIndex + PARALELLISM && i < shortlist.Count; i++) {
					asyncFindValue(shortlist.Values[i], target, ref conversationIds);
                }

                List<Contact> suggested = new List<Contact>();

                DateTime called = DateTime.Now;
			    while(DateTime.Now < called.Add(new TimeSpan(max_time*conversationIds.Count))) {
				    // See if we got data!
				    findDataResponseCache(ref conversationIds, ref vals);
				    // If we got a contact, send it up
				    findContactResponseCache(ref conversationIds, ref suggested);
				    Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
			    }

                int y = 0;
                foreach (ID id in conversationIds.Keys)
                {
                    if (!conversationIds[id])
                    {
                        // Node down. Remove from shortlist and adjust loop indicies
                        log.Info("Node is down. Removing it from shortlist!");
                        shortlist.RemoveAt(y);
                        y--;
                        shortlistIndex--;
                    }
                    y++;
                }
				// But first, we have to store it at the closest node that doesn't have it yet.
				// TODO: Actually do that. Not doing it now since we don't have the publish time.
				return shortlist.Values;
			}
			
			// Drop extra ones
			// TODO: This isn't what the protocol says at all.
			while(shortlist.Count > NODES_TO_FIND) {
				shortlist.RemoveAt(shortlist.Count - 1);
			}
			return shortlist.Values;
		}

        /// <summary>
        /// Do an iterativeFindValue.
        /// If we find values, we return them and put null in close.
        /// If we don't, we return null and put a list of close nodes in close.
        /// Note that this will NOT EVER CHECK THE LOCAL NODE! DO IT YOURSELF!
        /// </summary>
        /// <param name="target"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        private void IterativeFindValue(string target, ref IList<KademliaResource> found, out IList<Contact> close)
        {
            close = IterativeFindValue(target, ref found);
        }
		#endregion
		
		#region Synchronous Operations
		/// <summary>
		/// Try to store something at the given node.
		/// No return so we just pretend it's synchronous
		/// </summary>
		/// <param name="storeAt"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <param name="originalInsertion"></param>
		private void SyncStore(Contact storeAt, CompleteTag tag, DateTime originalInsertion, EndpointAddress endpoint)
		{
			// Make a message
            ID tagID = ID.FromString(tag.TagHash);
			StoreQuery storeIt = new StoreQuery(nodeID, tagID, originalInsertion, endpoint.Uri);
			
			// Record having sent it
			OutstandingStoreRequest req = new OutstandingStoreRequest();
            req.key = tagID;
			req.val = tag;
			req.sent = DateTime.Now;
			req.publication = originalInsertion;
			lock(sentStoreRequests) {
				sentStoreRequests[storeIt.ConversationID] = req;
			}
			
			// Send it
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(storeAt.NodeEndPoint)
            );
            svc.HandleStoreQuery(storeIt);
			//SendMessage(storeAt, storeIt);
		}
		
		/// <summary>
		/// Send a FindNode request, and return an ID to retrieve its response.
		/// </summary>
		/// <param name="ask"></param>
		/// <param name="toFind"></param>
		/// <returns></returns>
		private void asyncFindNode(Contact ask, ID toFind, ref Dictionary<ID, bool> conversationIds)
		{
			// Send message

			FindNode question = new FindNode(nodeID, toFind, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(ask.NodeEndPoint)
            );
            svc.HandleFindNode(question);

            conversationIds[question.ConversationID] = false;
		}
		
		/// <summary>
		/// Send a synchronous FindValue.
		/// If we get a contact list, it gets returned.
		/// If we get data, it comes out in val and we return null.
		/// If we get nothing, we return null and val is null.
		/// </summary>
		/// <param name="ask"></param>
		/// <param name="toFind"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		private void asyncFindValue(Contact ask, string toFind, ref Dictionary<ID, bool> conversationIds)
		{
			// Send message
			FindValue question = new FindValue(nodeID, toFind, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(ask.NodeEndPoint)
            );
            svc.HandleFindValue(question);

            conversationIds[question.ConversationID] = false;
        }

        private void asyncPing(EndpointAddress toPing, ref Dictionary<ID, bool> conversationIds)
        {
            // Send message
            Ping ping = new Ping(nodeID, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), toPing
            );
            svc.HandlePing(ping);

            conversationIds[ping.ConversationID] = false;
        }

		/// <summary>
		/// Send a ping and wait for a response or a timeout.
		/// </summary>
		/// <param name="ping"></param>
		/// <returns>true on a respinse, false otherwise</returns>
		private bool SyncPing(EndpointAddress toPing)
		{
			// Send message
			Ping ping = new Ping(nodeID, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), toPing
            );
            svc.HandlePing(ping);

            DateTime called = DateTime.Now;
			while(DateTime.Now < called.Add(MAX_SYNC_WAIT)) {
				// If we got a response, send it up
				Pong resp = GetCachedResponse<Pong>(ping.ConversationID);
				if(resp != null) {
					return true; // They replied in time
				}
				Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
			}
			log.Info("Ping timeout");
			return false; // Nothing in time
		}
		#endregion

        #region Events Delegates
        private void handlePingDelegate(Object o)
        {
            Ping ping = (Ping)o;
            HandleMessage(ping);
            Console.WriteLine("Handling ping from: " + ping.NodeEndpoint);
            Pong pong = new Pong(nodeID, ping, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(ping.NodeEndpoint)
            );
            svc.HandlePong(pong);
        }

        private void handlePongDelegate(Object o)
        {
            Pong pong = (Pong)o;
            Console.WriteLine("Handling pong from: " + pong.NodeEndpoint);
            CacheResponse(pong);
        }

        private void handleFindNodeDelegate(Object o)
        {
            FindNode request = (FindNode)o;
            HandleMessage(request);
            List<Contact> closeNodes = contactCache.CloseContacts(request.Target, request.SenderID);
            FindNodeResponse response = new FindNodeResponse(nodeID, request, closeNodes, nodeEndpoint.Uri);
            IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                new NetUdpBinding(), new EndpointAddress(request.NodeEndpoint)
            );
            svc.HandleFindNodeResponse(response);
        }

        private void handleFindNodeResponseDelegate(Object o)
        {
            FindNodeResponse response = (FindNodeResponse)o;
            CacheResponse(response);
        }

        private void handleFindValueDelegate(Object o)
        {
            FindValue request = (FindValue)o;
            HandleMessage(request);
            log.Info("Searching for: " + request.Key);
            KademliaResource[] results = datastore.SearchFor(request.Key);
            if (results.Length > 0)
            {
                log.Info("Sending data to requestor: " + request.NodeEndpoint.ToString());
                FindValueDataResponse response = new FindValueDataResponse(nodeID, request, results, nodeEndpoint.Uri);
                IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                    new NetUdpBinding(), new EndpointAddress(request.NodeEndpoint)
                );
                svc.HandleFindValueDataResponse(response);
            }
            else
            {
                List<Contact> closeNodes = contactCache.CloseContacts(ID.Hash(request.Key), request.SenderID);
                FindValueContactResponse response = new FindValueContactResponse(nodeID, request, closeNodes, nodeEndpoint.Uri);
                IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                    new NetUdpBinding(), new EndpointAddress(request.NodeEndpoint)
                );
                svc.HandleFindValueContactResponse(response);
            }
        }

        private void handleStoreQueryDelegate(Object o)
        {
            StoreQuery request = (StoreQuery)o;
            HandleMessage(request);

            if (!datastore.ContainsUrl(request.TagHash.ToString(), request.NodeEndpoint))
            {
                acceptedStoreRequests[request.ConversationID] = DateTime.Now; // Record that we accepted it
                StoreResponse response = new StoreResponse(nodeID, request, true, nodeEndpoint.Uri);
                IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                    new NetUdpBinding(), new EndpointAddress(request.NodeEndpoint)
                );
                svc.HandleStoreResponse(response);
            }
            else if (request.PublicationTime > datastore.GetPublicationTime(request.TagHash.ToString(), request.NodeEndpoint)
                    && request.PublicationTime < DateTime.Now.ToUniversalTime().Add(MAX_CLOCK_SKEW))
            {
                datastore.RefreshResource(request.TagHash.ToString(), request.NodeEndpoint, request.PublicationTime);
            }
        }

        private void handleStoreDataDelegate(Object o)
        {
            StoreData request = (StoreData)o;
            HandleMessage(request);
            // If we asked for it, store it and clear the authorization.
            lock (acceptedStoreRequests)
            {
                if (acceptedStoreRequests.ContainsKey(request.ConversationID))
                {
                    //acceptedStoreRequests.Remove(request.Key);
                    acceptedStoreRequests.Remove(request.ConversationID);

                    // TODO: Calculate when we should expire this data according to Kademlia
                    // For now just keep it until it expires

                    // Don't accept stuff published far in the future
                    if (request.PublicationTime < DateTime.Now.ToUniversalTime().Add(MAX_CLOCK_SKEW))
                    {
                        // We re-hash since we shouldn't trust their hash
                        //	datastore.Put(request.Key, ID.Hash(request.Data), request.Data, request.PublicationTime, EXPIRE_TIME);
                        Console.WriteLine("Arrived store data from peer with transport: " + request.TransportUri);
                        datastore.StoreResource(request.Data, request.TransportUri, request.PublicationTime);
                    }
                }
            }
        }

        private void handleStoreResponseDelegate(Object o)
        {
            StoreResponse response = (StoreResponse)o;
            CacheResponse(response);
            lock (sentStoreRequests)
            {
                if (response.ShouldSendData
                   && sentStoreRequests.ContainsKey(response.ConversationID))
                {
                    // We actually sent this
                    // Send along the data and remove it from the list
                    OutstandingStoreRequest toStore = sentStoreRequests[response.ConversationID];
                    StoreData r = new StoreData(nodeID, response, toStore.val, toStore.publication, nodeEndpoint.Uri, transportEndpoint.Uri);
                    Console.WriteLine("Transport Endpoint transmitted => " + r.TransportUri);
                    IKademliaNode svc = ChannelFactory<IKademliaNode>.CreateChannel(
                        new NetUdpBinding(), new EndpointAddress(response.NodeEndpoint)
                    );
                    svc.HandleStoreData(r);
                    sentStoreRequests.Remove(response.ConversationID);
                }
            }
        }

        private void handleFindValueContactResponseDelegate(Object o)
        {
            FindValueContactResponse response = (FindValueContactResponse)o;
            CacheResponse(response);
        }

        private void handleFindValueDataResponseDelegate(Object o)
        {
            FindValueDataResponse response = (FindValueDataResponse)o;
            CacheResponse(response);
        }
        #endregion

        #region Protocol Events
        /// <summary>
		/// Record every contact we see in our cache, if applicable. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="msg"></param>
		public void HandleMessage(Message msg)
		{
			log.Info(nodeID.ToString() + " got " + msg.Name + " from " + msg.SenderID.ToString());
			SawContact(new Contact(msg.SenderID,msg.NodeEndpoint));
		}
		
		/// <summary>
		/// Store responses in the response cache to be picked up by threads waiting for them
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="response"></param>
		public void CacheResponse(Response response)
		{
            HandleMessage(response);
            log.Info("Caching response");
			CachedResponse entry = new CachedResponse();
			entry.arrived = DateTime.Now;
			entry.response = response;
			//Log("Caching " + response.GetName() + " under " + response.GetConversationID().ToString());
			// Store in cache
            responseCacheLocker.WaitOne();
		    responseCache[response.ConversationID] = entry;
			responseCacheLocker.Set();
		}
		
		/// <summary>
		/// Respond to a ping by sending a pong
		/// </summary>
		/// <param name="p"></param>
		public void HandlePing(Ping ping)
		{
            ThreadPool.QueueUserWorkItem(new WaitCallback(handlePingDelegate), ping);
		}

        public void HandlePong(Pong pong)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(handlePongDelegate), pong);
        }
		
		/// <summary>
		/// Send back the contacts for the K closest nodes to the desired ID, not including the requester.
		/// K = BucketList.BUCKET_SIZE;
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		public void HandleFindNode(FindNode request)
		{
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleFindNodeDelegate), request);
		}

        public void HandleFindNodeResponse(FindNodeResponse response)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleFindNodeResponseDelegate), response);
        }
		
		/// <summary>
		/// Give the value if we have it, or the closest nodes if we don't.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		public void HandleFindValue(FindValue request)
		{
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleFindValueDelegate), request);
		}
		
		/// <summary>
		/// Ask for data if we don't already have it. Update time info if we do.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		public void HandleStoreQuery(StoreQuery request)
		{
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleStoreQueryDelegate), request);
		}
		
		/// <summary>
		/// Store the data, if we requested it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		public void HandleStoreData(StoreData request)
		{
            Console.WriteLine("Arrived a storeData from " + request.TransportUri);
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleStoreDataDelegate), request);
		}
		
		/// <summary>
		/// Send data in response to affirmative SendResponses
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		public void HandleStoreResponse(StoreResponse response)
		{
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleStoreResponseDelegate), response);
		}
		
        public void HandleFindValueContactResponse(FindValueContactResponse response)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleFindValueContactResponseDelegate), response);
        }

        public void HandleFindValueDataResponse(FindValueDataResponse response)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(handleFindValueDataResponseDelegate), response);
        }

		/// <summary>
		/// Expire entries in the accepted/sent store request caches and the response cache.
		/// </summary>
		private void MindCaches()
		{
            log.Info("Starting cache manager");
			while(true) {
				// Do accepted requests
				lock(acceptedStoreRequests) {
					for(int i = 0; i < acceptedStoreRequests.Count; i++) {
						// Remove stuff that is too old
						if(DateTime.Now.Subtract(acceptedStoreRequests.Values[i]) > MAX_CACHE_TIME) {
							acceptedStoreRequests.RemoveAt(i);
							i--;
						}
					}
				}
				
				// Do sent requests
				lock(sentStoreRequests) {
					for(int i = 0; i < sentStoreRequests.Count; i++) {
						if(DateTime.Now.Subtract(sentStoreRequests.Values[i].sent) > MAX_CACHE_TIME) {
							sentStoreRequests.RemoveAt(i);
							i--;
						}
					}
				}
				
				// Do responses
				responseCacheLocker.WaitOne();
				for(int i = 0; i < responseCache.Count; i++) {
					if(DateTime.Now.Subtract(responseCache.Values[i].arrived) > MAX_CACHE_TIME) {
						responseCache.RemoveAt(i);
						i--;
					}
				}
                responseCacheLocker.Set();
				
				Thread.Sleep(CHECK_INTERVAL);
			}
		}
		
		#endregion

        private void findContactResponseCache(ref Dictionary<ID, bool> toSearch, ref List<Contact> vals)
        {
            log.Debug("Searching for contact in cache!");
            responseCacheLocker.WaitOne();
            List<ID> keys = new List<ID>(toSearch.Keys);
            for (int i = 0; i < toSearch.Count; i++)
            {
                ID cId = keys[i];
                if (responseCache.ContainsKey(cId))
                {
                    try
                    {
                        toSearch[cId] = true;
                        foreach (Contact c in ((FindValueContactResponse)responseCache[cId].response).Contacts)
                        {
                            vals.Add(c);
                        }
                        responseCache.Remove(cId);
                    }
                    catch (Exception) { }
                }
            }
            responseCacheLocker.Set();
        }

        private void findDataResponseCache(ref Dictionary<ID, bool> toSearch, ref IList<KademliaResource> vals)
        {
            log.Debug("Searching for data in cache!");
            responseCacheLocker.WaitOne();
            List<ID> keys = new List<ID>(toSearch.Keys);
            for (int i = 0; i < toSearch.Count; i++)
            {
                ID cId = keys[i];
                if (responseCache.ContainsKey(cId))
                {
                    try
                    {
                        toSearch[cId] = true;
                        foreach (KademliaResource c in ((FindValueDataResponse)responseCache[cId].response).Values)
                        {
                            vals.Add(c);
                            Console.WriteLine("Found resource " + c.Id);
                        }
                        responseCache.Remove(cId);
                    }
                    catch (Exception) { }
                }
            }
            responseCacheLocker.Set();
        }

        private void findPingResponseCache(ref Dictionary<ID, bool> toSearch)
        {
            responseCacheLocker.WaitOne();
            List<ID> keys = new List<ID>(toSearch.Keys);
            for(int i = 0; i< toSearch.Count; i++)
            {
                ID cID = keys[i];
                if (responseCache.ContainsKey(cID))
                {
                    toSearch[cID] = true;
                    responseCache.Remove(cID);
                }
            }
            responseCacheLocker.Set();
        }

        private void findNodeResponseCache(ref Dictionary<ID, bool> toSearch, ref List<Contact> suggested)
        {
            responseCacheLocker.WaitOne();
            List<ID> keys = new List<ID>(toSearch.Keys);
            for (int i = 0; i < toSearch.Count; i++)
            {
                ID cId = keys[i];
                if (responseCache.ContainsKey(cId))
                {
                    try
                    {
                        toSearch[cId] = true;
                        foreach (Contact c in ((FindNodeResponse)responseCache[cId].response).Contacts)
                        {
                            suggested.Add(c);
                        }
                        responseCache.Remove(cId);
                    }
                    catch (Exception) { }
                }
            }
            responseCacheLocker.Set();
        }

        /// <summary>
        /// Get a properly typed response from the cache, or null if none exists.
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        private T GetCachedResponse<T>(ID conversation) where T : Response
        {
            responseCacheLocker.WaitOne();
            if (responseCache.ContainsKey(conversation))
            { // If we found something of the right type
                try
                {
                    T toReturn = (T)responseCache[conversation].response;
                    responseCache.Remove(conversation);
                    //Log("Retrieving cached " + toReturn.GetName());
                    responseCacheLocker.Set();
                    return toReturn; // Pull it out and return it
                }
                catch (Exception ex)
                {
                    // Couldn't actually cast to type we want.
                    responseCacheLocker.Set();
                    return null;
                }
            }
            else
            {
                //Log("Nothing for " + conversation.ToString());
                responseCacheLocker.Set();
                return null; // Nothing there -> null
            }
        }

		#region Framework
		
		/// <summary>
		/// Call this whenever we see a contact.
		/// We add the contact to the queue to be cached.
		/// </summary>
		/// <param name="seen"></param>
		private void SawContact(Contact seen)
		{
            if (seen.NodeID == nodeID)
            {
				return; // NEVER insert ourselves!
			}
			
			lock(contactQueue) {
				if(contactQueue.Count < MAX_QUEUE_LENGTH) { // Don't let it get too long
					contactQueue.Add(seen);
				}
				
			}
		}
		
		/// <summary>
		/// Run in the background and add contacts to the cache.
		/// </summary>
		private void MindBuckets()
		{
            log.Info("Starting buckets periodic manager.");
			while(true) {
				
				// Handle all the queued contacts
				while(contactQueue.Count > 0) {
					Contact applicant;
					lock(contactQueue) { // Only lock when getting stuff.
						applicant = contactQueue[0];
						contactQueue.RemoveAt(0);
					}
					
					//Log("Processing contact for " + applicant.GetID().ToString());
					
					// If we already know about them
                    if (contactCache.Contains(applicant.NodeID))
                    {
						// If they have a new address, record that
                        if (contactCache.Get(applicant.NodeID).NodeEndPoint != applicant.NodeEndPoint) 
                        {
							// Replace old one
                            contactCache.Remove(applicant.NodeID);
							contactCache.Put(applicant);
						}
                        else // Just promote them
                        { 
                            contactCache.Promote(applicant.NodeID);
						}
						continue;
					}
					
					// If we can fit them, do so
					Contact blocker = contactCache.Blocker(applicant.NodeID);
					if(blocker == null) {
						contactCache.Put(applicant);
					} else {
						// We can't fit them. We have to choose between blocker and applicant
						if(!SyncPing(new EndpointAddress(blocker.NodeEndPoint))) { // If the blocker doesn't respond, pick the applicant.
                            contactCache.Remove(blocker.NodeID);
							contactCache.Put(applicant);
							log.Info("Choose applicant");
						} else {
							log.Info("Choose blocker");
						}
					}
					
					//Log(contactCache.ToString());
				}
				
				// Wait for more
				Thread.Sleep(CHECK_INTERVAL);
			}
		}
		#endregion
		
	}
}
