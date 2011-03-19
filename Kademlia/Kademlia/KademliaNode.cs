/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 7:13 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Kademlia.Messages;

namespace Kademlia
{
	/// <summary>
	/// Functions as a peer in the overlay network.
	/// </summary>
	public class KademliaNode
	{
		// Identity
		private ID nodeID;
		
		// Network State
		private BucketList contactCache;
		private Thread bucketMinder; // Handle updates to cache
		private const int CHECK_INTERVAL = 1;
		private List<Contact> contactQueue; // Add contacts here to be considered for caching
		private const int MAX_QUEUE_LENGTH = 10;
		
		// Response cache
		// We want to be able to synchronously wait for responses, so we have other threads put them in this cache.
		// We also need to discard old ones.
		private struct CachedResponse {
			public Response response;
			public DateTime arrived;
		}
		private SortedList<ID, CachedResponse> responseCache;
		private TimeSpan MAX_SYNC_WAIT = new TimeSpan(5000000); // 500 ms in ticks
		
		// Application (datastore)
		private LocalStorage datastore; // Keep our key/value pairs
		private SortedList<ID, DateTime> acceptedStoreRequests; // Store a list of what put requests we actually accepted while waiting for data.
		// The list of put requests we sent is more complex
		// We need to keep the data and timestamp, but don't want to insert it in our storage.
		// So we keep it in a cache, and discard it if it gets too old.
		private struct OutstandingStoreRequest {
			public ID key;
			public string val;
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
		
		// Network IO
		private UdpClient client;
		private Thread clientMinder;
		
		// Events
		// Messages are strongly typed. Hooray!
		public event MessageEventHandler<Message> GotMessage;
		public event MessageEventHandler<Response> GotResponse;
		
		public event MessageEventHandler<Ping> GotPing;
		public event MessageEventHandler<Pong> GotPong;
		public event MessageEventHandler<FindNode> GotFindNode;
		public event MessageEventHandler<FindNodeResponse> GotFindNodeResponse;
		public event MessageEventHandler<FindValue> GotFindValue;
		public event MessageEventHandler<FindValueContactResponse> GotFindValueContactResponse;
		public event MessageEventHandler<FindValueDataResponse> GotFindValueDataResponse;
		public event MessageEventHandler<StoreQuery> GotStoreQuery;
		public event MessageEventHandler<StoreResponse> GotStoreResponse;
		public event MessageEventHandler<StoreData> GotStoreData;
		
		private bool debug = false;
		
		#region Setup	
		
		/// <summary>
		/// Make a node on a random available port, using an ID specific to this machine.
		/// </summary>
		public KademliaNode() : this(0, ID.HostID())
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Make a node with a specified ID.
		/// </summary>
		/// <param name="id"></param>
		public KademliaNode(ID id) : this(0, id)
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Make a node on a specified port.
		/// </summary>
		/// <param name="port"></param>
		public KademliaNode(int port) : this(port, ID.HostID())
		{
			// Nothing to do!
		}
		
		/// <summary>
		/// Make a node on a specific port, with a specified ID
		/// </summary>
		/// <param name="port"></param>
		public KademliaNode(int port, ID id)
		{
			// Set up all our data
			nodeID = id;
			contactCache = new BucketList(nodeID);
			contactQueue = new List<Contact>();
			datastore = new LocalStorage();
			acceptedStoreRequests = new SortedList<ID, DateTime>();
			sentStoreRequests = new SortedList<ID, KademliaNode.OutstandingStoreRequest>();
			responseCache = new SortedList<ID, KademliaNode.CachedResponse>();
			lastReplication = default(DateTime);

			// Start minding the buckets
			bucketMinder = new Thread(new ThreadStart(MindBuckets));
			bucketMinder.IsBackground = true;
			bucketMinder.Start();
			
			// Start minding the conversation state caches
			authMinder = new Thread(new ThreadStart(MindCaches));
			authMinder.IsBackground = true;
			authMinder.Start();
			
			// Set all the event handlers
			GotMessage += HandleMessage;
			GotResponse += CacheResponse;
			
			GotPing += HandlePing;
			GotFindNode += HandleFindNode;
			GotFindValue += HandleFindValue;
			GotStoreQuery += HandleStoreQuery;
			GotStoreResponse += HandleStoreResponse;
			GotStoreData += HandleStoreData;
			
			// Connect
			client = new UdpClient(port);
			clientMinder = new Thread(new ThreadStart(MindClient));
			clientMinder.IsBackground = true;
			clientMinder.Start();
			
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
		public bool Bootstrap(int localPort)
		{
			return Bootstrap(new IPEndPoint(IPAddress.Loopback, localPort));
		}
			
		/// <summary>
		/// Bootstrap the node by having it ping another node. Returns true if we get a response.
		/// </summary>
		/// <param name="other"></param>
		public bool Bootstrap(IPEndPoint other)
		{
			// Send a blocking ping.
			bool worked = SyncPing(other);
			Thread.Sleep(CHECK_INTERVAL); // Wait for them to notice us
			return worked;
		}
		
		/// <summary>
		/// Join the network.
		/// Assuming we have some contacts in our cache, get more by IterativeFindNoding ourselves.
		/// Then, refresh most (TODO: all) buckets.
		/// Returns true if we are connected after all that, false otherwise.
		/// </summary>
		public bool JoinNetwork() {
			Log("Joining");
			IList<Contact> found = IterativeFindNode(nodeID);
			if(found == null) {
				Log("Found <null list>");
			} else {
				foreach(Contact c in found) {
					Log("Found contact: " + c.ToString());
				}
			}
			
			
			// Should get very nearly all of them
			// RefreshBuckets(); // Put this off until first maintainance.
			if(contactCache.GetCount() > 0) {
				Log("Joined");
				return true;
			} else {
				Log("Failed to join! No other nodes known!");
				return false;
			}
		}
		#endregion
		
		#region Interface
		/// <summary>
		/// Enables degugging output for the node.
		/// </summary>
		public void EnableDebug() 
		{
			this.debug = true;
		}
		
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
			return ((IPEndPoint) client.Client.LocalEndPoint).Port;
		}
		
		/// <summary>
		/// Store something in the DHT as the original publisher.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void Put(ID key, string val)
		{
			IterativeStore(key, val, DateTime.Now);
			// TODO: republish
		}
		
		/// <summary>
		/// Gets values for a key from the DHT. Returns the values or an empty list.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<string> Get(ID key)
		{
			if(datastore.ContainsKey(key)) { // Check the local datastore first.
				return datastore.Get(key);
			} else {
				IList<Contact> close;
				IList<string> found = IterativeFindValue(key, out close);
				if(found == null) { // Empty list for nothing found
					return new List<string>();
				} else {
					return found;
				}
			}
		}
		#endregion
		
		#region Maintainance Operations
		/// <summary>
		/// Expire old data, replicate all data, refresh needy buckets.
		/// </summary>
		private void MindMaintainance()
		{
			while(true) {
				Thread.Sleep(MAINTAINANCE_INTERVAL);
				Log("Performing maintainance");
				// Expire old
				datastore.Expire();
				Log(datastore.GetKeys().Count + " keys stored.");
				
				// Replicate all if needed
				// We get our own lists to iterate
				if(DateTime.Now > lastReplication.Add(REPLICATE_TIME)) {
					Log("Replicating");
					foreach(ID key in datastore.GetKeys()) {
						foreach(ID valHash in datastore.GetHashes(key)) {
							try {
								IterativeStore(key, datastore.Get(key, valHash), (DateTime) datastore.GetPublicationTime(key, valHash));
							} catch (Exception ex) {
								Log("Could not replicate " + key.ToString() + "/" + valHash.ToString() + ": " + ex.ToString());
							}
						}
						
					}
					lastReplication = DateTime.Now;
				}
				
				// Refresh any needy buckets
				RefreshBuckets();
				Log("Done");
			}
		}
		
		/// <summary>
		/// Look for nodes to go in buckets we haven't touched in a while.
		/// </summary>
		private void RefreshBuckets()
		{
			Log("Refreshing buckets");
			IList<ID> toLookup = contactCache.IDsForRefresh(REFRESH_TIME);
			foreach(ID key in toLookup) {
				IterativeFindNode(key);
			}
			Log("Refreshed");
		}
		
		#endregion
		
		#region Iterative Operations
		
		/// <summary>
		/// Do an iterativeStore operation and publish the key/value pair on the network
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		private void IterativeStore(ID key, string val, DateTime originalInsertion)
		{
			// Find the K closest nodes to the key
			IList<Contact> closest = IterativeFindNode(key);
			Log("Storing at " + closest.Count + " nodes");
			foreach(Contact c in closest) {
				// Store a copy at each
				SyncStore(c.GetEndPoint(), key, val, originalInsertion);
			}
		}
		
		/// <summary>
		/// Do an iterativeFindNode operation.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		private IList<Contact> IterativeFindNode(ID target)
		{
			IList<string> temp;
			return IterativeFind(target, false, out temp);
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
		private IList<string> IterativeFindValue(ID target, out IList<Contact> close)
		{
			IList<string> found;
			close = IterativeFind(target, true, out found);
			return found;
		}
		
		/// <summary>
		/// Perform a Kademlia iterativeFind* operation.
		/// If getValue is true, it sends out a list of strings if values are found, or null none are.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="getValue">true for FindValue, false for FindNode</param>
		/// <param name="vals"></param>
		/// <returns></returns>
		private IList<Contact> IterativeFind(ID target, bool getValue, out IList<string> vals)
		{
			// Log the lookup
			if(target != nodeID) {
				contactCache.Touch(target);
			}
			
			// Get the alpha closest nodes to the target
			// TODO: Should actually pick from a certain bucket.
			SortedList<ID, Contact> shortlist = new SortedList<ID, Contact>();
			foreach(Contact c in contactCache.CloseContacts(PARALELLISM, target, null)) {
				shortlist.Add(c.GetID(), c);
			}
			
			int shortlistIndex = 0; // Everyone before this is up.
			
			// Make an initial guess for the closest node
			Contact closest = null;
			foreach(Contact toAsk in shortlist.Values) {
				if(closest == null || (toAsk.GetID() ^ target) < (closest.GetID() ^ target)) {
					closest = toAsk;
				}
			}
			
			// Until we run out of people to ask or we're done...
			while(shortlistIndex < shortlist.Count && shortlistIndex < NODES_TO_FIND) {
				// Try the first alpha unexamined contacts
				bool foundCloser = false; // TODO: WTF does the spec want with this
				for(int i = shortlistIndex; i < shortlistIndex + PARALELLISM && i < shortlist.Count; i++) {
					List<Contact> suggested;
					if(getValue) { // Get list or value
						IList<string> returnedValues = null;
						suggested = SyncFindValue(shortlist.Values[i].GetEndPoint(), target, out returnedValues);
						if(returnedValues != null) { // We found it! Pass it up!
							vals = returnedValues;
							// But first, we have to store it at the closest node that doesn't have it yet.
							// TODO: Actually do that. Not doing it now since we don't have the publish time.
							return null;
						}
					} else { // Only get list
						suggested = SyncFindNode(shortlist.Values[i].GetEndPoint(), target);
					}
					
					if(suggested != null) {
						// Add suggestions to shortlist and check for closest
						foreach(Contact suggestion in suggested) {
							if(!shortlist.ContainsKey(suggestion.GetID())) { // Contacts aren't value types so we have to do this.
								shortlist.Add(suggestion.GetID(), suggestion);
							}
							if((suggestion.GetID() ^ target) < (closest.GetID() ^ target)) {
								closest = suggestion;
								foundCloser = true;
							}
						}
					} else {
						// Node down. Remove from shortlist and adjust loop indicies
						shortlist.RemoveAt(i);
						i--;
						shortlistIndex--;
					}
				}
				shortlistIndex += PARALELLISM;
			}
			
			// Drop extra ones
			// TODO: This isn't what the protocol says at all.
			while(shortlist.Count > NODES_TO_FIND) {
				shortlist.RemoveAt(shortlist.Count - 1);
			}
			vals = null;
			return shortlist.Values;
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
		private void SyncStore(IPEndPoint storeAt, ID key, string val, DateTime originalInsertion)
		{
			// Make a message
			Message storeIt = new StoreQuery(nodeID, key, ID.Hash(val), originalInsertion, val.Length);
			
			// Record having sent it
			OutstandingStoreRequest req = new OutstandingStoreRequest();
			req.key = key;
			req.val = val;
			req.sent = DateTime.Now;
			req.publication = originalInsertion;
			lock(sentStoreRequests) {
				sentStoreRequests[storeIt.GetConversationID()] = req;
			}
			
			// Send it
			SendMessage(storeAt, storeIt);
		}
		
		/// <summary>
		/// Send a FindNode request, and return an ID to retrieve its response.
		/// </summary>
		/// <param name="ask"></param>
		/// <param name="toFind"></param>
		/// <returns></returns>
		private List<Contact> SyncFindNode(IPEndPoint ask, ID toFind)
		{
			// Send message
			DateTime called = DateTime.Now;
			Message question = new FindNode(nodeID, toFind);
			SendMessage(ask, question);
			
			while(DateTime.Now < called.Add(MAX_SYNC_WAIT)) {
				// If we got a response, send it up
				FindNodeResponse resp = GetCachedResponse<FindNodeResponse>(question.GetConversationID());
				if(resp != null) {
					return resp.GetContacts();
				}
				Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
			}
			return null; // Nothing in time
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
		private List<Contact> SyncFindValue(IPEndPoint ask, ID toFind, out IList<string> val)
		{
			// Send message
			DateTime called = DateTime.Now;
			Message question = new FindValue(nodeID, toFind);
			SendMessage(ask, question);
			
			while(DateTime.Now < called.Add(MAX_SYNC_WAIT)) {
				// See if we got data!
				FindValueDataResponse dataResp = GetCachedResponse<FindValueDataResponse>(question.GetConversationID());
				if(dataResp != null) {
					// Send it out and return null!
					val = dataResp.GetValues();
					return null;
				}
				// If we got a contact, send it up
				FindValueContactResponse resp = GetCachedResponse<FindValueContactResponse>(question.GetConversationID());
				if(resp != null) {
					val = null;
					return resp.GetContacts();
				}
				Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
			}
			// Nothing in time
			val = null;
			return null; 
		}
		
		/// <summary>
		/// Send a ping and wait for a response or a timeout.
		/// </summary>
		/// <param name="ping"></param>
		/// <returns>true on a respinse, false otherwise</returns>
		private bool SyncPing(IPEndPoint toPing)
		{
			// Send message
			DateTime called = DateTime.Now;
			Message ping = new Ping(nodeID);
			SendMessage(toPing, ping);
			
			while(DateTime.Now < called.Add(MAX_SYNC_WAIT)) {
				// If we got a response, send it up
				Pong resp = GetCachedResponse<Pong>(ping.GetConversationID());
				if(resp != null) {
					return true; // They replied in time
				}
				Thread.Sleep(CHECK_INTERVAL); // Otherwise wait for one
			}
			Log("Ping timeout");
			return false; // Nothing in time
		}
		#endregion
		
		#region Protocol Events
		/// <summary>
		/// Record every contact we see in our cache, if applicable. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="msg"></param>
		private void HandleMessage(Contact sender, Message msg)
		{
			Log(nodeID.ToString() + " got " + msg.GetName() + " from " + msg.GetSenderID().ToString());
			SawContact(sender);
		}
		
		/// <summary>
		/// Store responses in the response cache to be picked up by threads waiting for them
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="response"></param>
		private void CacheResponse(Contact sender, Response response)
		{
			CachedResponse entry = new CachedResponse();
			entry.arrived = DateTime.Now;
			entry.response = response;
			//Log("Caching " + response.GetName() + " under " + response.GetConversationID().ToString());
			// Store in cache
			lock(responseCache) {
				responseCache[response.GetConversationID()] = entry;
			}
		}
		
		/// <summary>
		/// Get a properly typed response from the cache, or null if none exists.
		/// </summary>
		/// <param name="conversation"></param>
		/// <returns></returns>
		private T GetCachedResponse<T>(ID conversation) where T : Response 
		{
			lock(responseCache) {
				if(responseCache.ContainsKey(conversation)) { // If we found something of the right type
					try {
						T toReturn = (T) responseCache[conversation].response;
						responseCache.Remove(conversation);
						//Log("Retrieving cached " + toReturn.GetName());
						return toReturn; // Pull it out and return it
					} catch (Exception ex) {
						// Couldn't actually cast to type we want.
						return null;
					}
				} else {
					//Log("Nothing for " + conversation.ToString());
					return null; // Nothing there -> null
				}
			}
		}
		
		/// <summary>
		/// Respond to a ping by sending a pong
		/// </summary>
		/// <param name="p"></param>
		private void HandlePing(Contact sender, Ping ping)
		{
			Message pong = new Pong(nodeID, ping);
			SendMessage(sender.GetEndPoint(), pong);
		}
		
		/// <summary>
		/// Send back the contacts for the K closest nodes to the desired ID, not including the requester.
		/// K = BucketList.BUCKET_SIZE;
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		private void HandleFindNode(Contact sender, FindNode request)
		{
			List<Contact> closeNodes = contactCache.CloseContacts(request.GetTarget(), sender.GetID());
			Message response = new FindNodeResponse(nodeID, request, closeNodes);
			SendMessage(sender.GetEndPoint(), response);
		}
		
		/// <summary>
		/// Give the value if we have it, or the closest nodes if we don't.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		private void HandleFindValue(Contact sender, FindValue request)
		{
			if(datastore.ContainsKey(request.GetKey())) {
				IList<string> vals = datastore.Get(request.GetKey());
				Message response = new FindValueDataResponse(nodeID, request, vals);
				SendMessage(sender.GetEndPoint(), response);
			} else {
				List<Contact> closeNodes = contactCache.CloseContacts(request.GetKey(), sender.GetID());
				Message response = new FindValueContactResponse(nodeID, request, closeNodes);
				SendMessage(sender.GetEndPoint(), response);
			}
		}
		
		/// <summary>
		/// Ask for data if we don't already have it. Update time info if we do.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		private void HandleStoreQuery(Contact sender, StoreQuery request)
		{
			//TODO: check size and local storage policy
			ID key = request.GetKey();
			ID valHash = request.GetDataHash();
			
			if(!datastore.Contains(key, valHash)) {
				acceptedStoreRequests[request.GetConversationID()] = DateTime.Now; // Record that we accepted it
				SendMessage(sender.GetEndPoint(), new StoreResponse(nodeID, request, true));
			} else if(request.GetPublicationTime() > datastore.GetPublicationTime(key, valHash)
			          && request.GetPublicationTime() < DateTime.Now.ToUniversalTime().Add(MAX_CLOCK_SKEW)) {
				// Update our recorded publicaton time
				datastore.Restamp(key, valHash, request.GetPublicationTime(), EXPIRE_TIME);
			}
		}
		
		/// <summary>
		/// Store the data, if we requested it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		private void HandleStoreData(Contact sender, StoreData request)
		{
			// If we asked for it, store it and clear the authorization.
			lock(acceptedStoreRequests) {
				if(acceptedStoreRequests.ContainsKey(request.GetConversationID())) {
					acceptedStoreRequests.Remove(request.GetKey());
					
					// TODO: Calculate when we should expire this data according to Kademlia
					// For now just keep it until it expires
					
					// Don't accept stuff published far in the future
					if(request.GetPublicationTime() < DateTime.Now.ToUniversalTime().Add(MAX_CLOCK_SKEW)) { 
						// We re-hash since we shouldn't trust their hash
						datastore.Put(request.GetKey(), ID.Hash(request.GetData()), request.GetData(), request.GetPublicationTime(), EXPIRE_TIME);
					}
				}
			}
		}
		
		/// <summary>
		/// Send data in response to affirmative SendResponses
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="request"></param>
		private void HandleStoreResponse(Contact sender, StoreResponse response)
		{
			lock(sentStoreRequests) {
				if(response.ShouldSendData() 
				   && sentStoreRequests.ContainsKey(response.GetConversationID())) {
					// We actually sent this
					// Send along the data and remove it from the list
					OutstandingStoreRequest toStore = sentStoreRequests[response.GetConversationID()];
					SendMessage(sender.GetEndPoint(), 
					            new StoreData(nodeID, response, toStore.key, ID.Hash(toStore.val), toStore.val, toStore.publication));
					sentStoreRequests.Remove(response.GetConversationID());
				}
			}
		}
		
		/// <summary>
		/// Expire entries in the accepted/sent store request caches and the response cache.
		/// </summary>
		private void MindCaches()
		{
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
				lock(responseCache) {
					for(int i = 0; i < responseCache.Count; i++) {
						if(DateTime.Now.Subtract(responseCache.Values[i].arrived) > MAX_CACHE_TIME) {
							responseCache.RemoveAt(i);
							i--;
						}
					}
				}
				
				Thread.Sleep(CHECK_INTERVAL);
			}
		}
		
		#endregion
		
		#region Framework
		/// <summary>
		/// Handle incoming packets
		/// </summary>
		private void MindClient() {
			while(true) {
				try {
					// Get a datagram
					IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); // Get from anyone
					byte[] data = client.Receive(ref sender);
					
					// Decode the message
					MemoryStream ms = new MemoryStream(data);
					IFormatter decoder = new BinaryFormatter();
					object got = null;
					try {
						got = decoder.Deserialize(ms);
					} catch(Exception ex) {
						Log("Invalid datagram!");
					}
					
					// Process the message
					if(got != null && got is Message) {
						DispatchMessageEvents(sender, (Message) got);
					} else {
						Log("Non-message object!");
					}
				} catch (Exception ex) {
					Log("Error recieving data: " + ex.ToString());
				}
			}
		}
		
		/// <summary>
		/// Sends out message events for the given message.
		/// ADD NEW MESSAGE TYPES HERE
		/// </summary>
		/// <param name="msg"></param>
		private void DispatchMessageEvents(IPEndPoint recievedFrom, Message msg) 
		{
			// Make a contact for the person who sent it.
			Contact sender = new Contact(msg.GetSenderID(), recievedFrom);
			
			// Every message gets this one
			if(GotMessage != null)
				GotMessage(sender, msg);
			
			// All responses get this one
			if(msg is Response && GotResponse != null)
				GotResponse(sender, (Response) msg);
			
			// All messages have special events
			// TODO: Dynamically register from each message class instead of this ugly elsif?
			if(msg is Ping) { // Pings
				if(GotPing != null)
					GotPing(sender, (Ping) msg);
			} else if(msg is Pong) { // Pongs
				if(GotPong != null)
					GotPong(sender, (Pong) msg);
			} else if(msg is FindNode) { // Node search
				if(GotFindNode != null)
					GotFindNode(sender, (FindNode) msg);
			} else if(msg is FindNodeResponse) {
				if(GotFindNodeResponse != null)
					GotFindNodeResponse(sender, (FindNodeResponse) msg);
			} else if(msg is FindValue) { // Key search
				if(GotFindValue != null)
					GotFindValue(sender, (FindValue) msg);
			} else if(msg is FindValueContactResponse) {
				if(GotFindValueContactResponse != null)
					GotFindValueContactResponse(sender, (FindValueContactResponse) msg);
			} else if(msg is FindValueDataResponse) {
				if(GotFindValueDataResponse != null)
					GotFindValueDataResponse(sender, (FindValueDataResponse) msg);
			} else if(msg is StoreQuery) {
				if(GotStoreQuery != null)
					GotStoreQuery(sender, (StoreQuery) msg);
			} else if(msg is StoreResponse) {
				if(GotStoreResponse != null)
					GotStoreResponse(sender, (StoreResponse) msg);
			} else if(msg is StoreData) {
				if(GotStoreData != null)
					GotStoreData(sender, (StoreData) msg);
			}
		}
		
		/// <summary>
		/// Send a mesaage to someone.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="to"></param>
		private void SendMessage(IPEndPoint destination, Message msg)
		{
			// Encode the message
			MemoryStream ms = new MemoryStream();
			IFormatter encoder = new BinaryFormatter();
			encoder.Serialize(ms, msg);
			byte[] messageData = ms.GetBuffer();
			
			Log(nodeID.ToString() + " sending " + msg.GetName() + " to " + destination.ToString());
			
			// Send it
			client.Send(messageData, messageData.Length, destination);
		}
		
		/// <summary>
		/// Call this whenever we see a contact.
		/// We add the contact to the queue to be cached.
		/// </summary>
		/// <param name="seen"></param>
		private void SawContact(Contact seen)
		{
			if(seen.GetID() == nodeID) {
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
					if(contactCache.Contains(applicant.GetID())) {
						// If they have a new address, record that
						if(contactCache.Get(applicant.GetID()).GetEndPoint()
						   != applicant.GetEndPoint()) {
							// Replace old one
							contactCache.Remove(applicant.GetID());
							contactCache.Put(applicant);
						} else { // Just promote them
							contactCache.Promote(applicant.GetID());
						}
						continue;
					}
					
					// If we can fit them, do so
					Contact blocker = contactCache.Blocker(applicant.GetID());
					if(blocker == null) {
						contactCache.Put(applicant);
					} else {
						// We can't fit them. We have to choose between blocker and applicant
						if(!SyncPing(blocker.GetEndPoint())) { // If the blocker doesn't respond, pick the applicant.
							contactCache.Remove(blocker.GetID());
							contactCache.Put(applicant);
							Log("Chose applicant");
						} else {
							Log("Chose blocker");
						}
					}
					
					//Log(contactCache.ToString());
				}
				
				// Wait for more
				Thread.Sleep(CHECK_INTERVAL);
			}
		}
		
		/// <summary>
		/// Log debug messages, if debugging is enabled.
		/// </summary>
		/// <param name="message"></param>
		private void Log(string message)
		{
			if(this.debug)
				Console.WriteLine(message);
		}
		#endregion
		
	}
}
