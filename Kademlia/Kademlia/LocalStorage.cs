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
 * Time: 7:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.IO;
using Persistence.Tag;
using Persistence;

namespace Kademlia
{

    public class DhtElement
    {
        public Uri url;
        public DateTime publication;
        TimeSpan validity;
    }

	/// <summary>
	/// Stores key/value pairs assigned to our node.
	/// Automatically handles persistence to disk.
	/// </summary>
	public class LocalStorage
	{
		[Serializable]
		private class Entry {
			// The existence of an entry implies a file with the data at PathFor(key, hash)
			public DateTime timestamp;
			public TimeSpan keepFor;
		}
		
		private SortedList<ID, SortedList<ID, Entry>> store; // TODO: Replace with real database.
		private Thread saveThread;
		private string indexFilename;
		private string storageRoot;
		private Mutex mutex;
		
		private const string INDEX_EXTENSION = ".index";
		private const string DATA_EXTENSION = ".dat";
		private static IFormatter coder = new BinaryFormatter(); // For disk storage
		private static TimeSpan SAVE_INTERVAL = new TimeSpan(0, 10, 0);
		
		/// <summary>
		/// Make a new LocalStorage. 
		/// Uses the executing assembly's name to determine the filename for on-disk storage.
		/// If another LocalStorage on the machine is already using that file, we use a temp directory.
		/// </summary>
		public LocalStorage()
		{
			string assembly = Assembly.GetEntryAssembly().GetName().Name;
			string libname = Assembly.GetExecutingAssembly().GetName().Name;
			
			// Check the mutex to see if we get the disk storage
			string mutexName = libname + "-" + assembly + "-storage";
			try {
				mutex = Mutex.OpenExisting(mutexName);
				// If that worked, our disk storage has to be in a temp directory
				storageRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			} catch(Exception ex) {
				// We get the real disk storage
				mutex = new Mutex(true, mutexName);				
				storageRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + libname + "\\" + assembly + "\\";
			}
			
			Console.WriteLine("Storing data in " + storageRoot);
			
			// Set a filename for an index file.
			indexFilename =  Path.Combine(storageRoot, "index" + INDEX_EXTENSION);
			
			// Get our store from disk, if possible
			if(File.Exists(indexFilename)) {
				try {
					// Load stuff from disk
					FileStream fs = File.OpenRead(indexFilename);
					store = (SortedList<ID, SortedList<ID, Entry>>) coder.Deserialize(fs);
					fs.Close();
				} catch (Exception ex) {
					Console.WriteLine("Could not load disk data: " + ex.ToString());
				}
			}
			
			// If we need a new store, make it
			if(store == null) {
				store = new SortedList<ID, SortedList<ID, Entry>>();
			}
			
			// Start the index autosave thread
			saveThread = new Thread(new ThreadStart(BackgroundSave));
			saveThread.IsBackground = true;
			saveThread.Start();
		}
		
		/// <summary>
		/// Clean up and close our mutex if needed.
		/// </summary>
		~LocalStorage()
		{
			saveThread.Abort(); // Stop our autosave thread.
			SaveIndex(); // Make sure our index getw written when we shut down properly.
			mutex.Close(); // Release our hold on the mutex.
		}
	
		/// <summary>
		/// Create all folders in a path, if missing.
		/// </summary>
		/// <param name="path"></param>
		private static void CreatePath(string path)
		{
			path = path.TrimEnd('/', '\\');
			if(Directory.Exists(path)) {
				return; // Base case
			} else {
				if(Path.GetDirectoryName(path) != "") {
					CreatePath(Path.GetDirectoryName(path)); // Make up to parent
				}
				Directory.CreateDirectory(path); // Make this one
			}
		}
		
		/// <summary>
		/// Where should we save a particular value?
		/// </summary>
		/// <param name="key"></param>
		/// <param name="hash"></param>
		/// <returns></returns>
		private string PathFor(ID key, ID hash)
		{
			return Path.Combine(Path.Combine(storageRoot, key.ToPathString()), hash.ToPathString() + DATA_EXTENSION);
		}
		
		/// <summary>
		/// Save the store in the background.
		/// PRECONSITION: We have the mutex and diskFilename is set.
		/// </summary>
		private void BackgroundSave()
		{
			while(true) {
				SaveIndex();
				Thread.Sleep(SAVE_INTERVAL);
			}
		}
		
		/// <summary>
		/// Save the index now.
		/// </summary>
		private void SaveIndex() 
		{
			try {
				Console.WriteLine("Saving datastore index...");
				CreatePath(Path.GetDirectoryName(indexFilename));
				
				// Save
				lock(store) {
					FileStream fs = File.OpenWrite(indexFilename);
					coder.Serialize(fs, store);
					fs.Close();
				}
				Console.WriteLine("Datastore index saved");
			} catch (Exception ex) { // Report errors so the thread keeps going
				Console.WriteLine("Save error: " + ex.ToString());
			}
		}

        public KademliaResource[] SearchFor(string query)
        {
            return new KademliaResource[10];
        }

        public void RefreshResource(ID tag_id, Uri uri, DateTime timestamp)
        {
        }

        public void StoreResource(CompleteTag tag, Uri uri, DateTime publicationTime)
        {
        }

        public void Put(ID tag_id, Uri uri, DateTime publicationTime)
        {
        }

        public bool ContainsTag(ID tag_id)
        {
            return false;
        }

        public bool ContainsUrl(ID tag_id, Uri uri)
        {
            return false;
        }
        /*
        IList<Uri> Get(ID tag_id)
        {
            return new List<Uri>();
        }
        */
        public DateTime GetPublicationTime(ID tag_id, Uri uri)
        {
            return new DateTime();
        }

        public IList<KademliaResource> GetAllElements()
        {
            return new List<KademliaResource>();
        }
		/// <summary>
		/// Store a key/value pair published originally at the given UTC timestamp. Value is kept until keepTime past timestamp.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="hash">The hash of the value</param>
		/// <param name="val"></param>
		/// <param name="timestamp"></param>
		/// <param name="keepTime"></param>
		public void Put(ID key, ID hash, string val, DateTime timestamp, TimeSpan keepFor)
		{
			// Write the file
			CreatePath(Path.GetDirectoryName(PathFor(key, hash)));
			File.WriteAllText(PathFor(key, hash), val);
			
			
			// Record its existence
			Entry entry = new Entry();
			entry.timestamp = timestamp;
			entry.keepFor = keepFor;
			
			lock(store) {
				if(!store.ContainsKey(key)) {
					store[key] = new SortedList<ID, Entry>();
				}
				store[key][hash] = entry;
			}
		}
		
		/// <summary>
		/// Change the timing information on an existing entry, if extant.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="hash"></param> 
		/// <param name="newStamp"></param>
		/// <param name="newKeep"></param>
		public void Restamp(ID key, ID hash, DateTime newStamp, TimeSpan newKeep)
		{
			lock(store) {
				if(store.ContainsKey(key) && store[key].ContainsKey(hash)) {
					store[key][hash].timestamp = newStamp;
					store[key][hash].keepFor = newKeep;
				}
			}
		}
		
		/// <summary>
		/// Do we have any data for the given key?
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(ID key)
		{
			return store.ContainsKey(key);
		}
		
		/// <summary>
		/// Do we have the specified value for the given key?
		/// </summary>
		/// <param name="key"></param>
		/// <param name="hash"></param>
		/// <returns></returns>
		public bool Contains(ID key, ID hash)
		{
			lock(store) {
				return store.ContainsKey(key) && store[key].ContainsKey(hash);
			}
		}
		
		/// <summary>
		/// Get all data values for the given key, or an empty list.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<string> Get(ID key)
		{
			List<string> toReturn = new List<string>();
			lock(store) {
				if(ContainsKey(key)) {
					foreach(ID hash in store[key].Keys) {
						// Load the value and add it to the list
						toReturn.Add(File.ReadAllText(PathFor(key, hash)));
					}
				}
			}
			return toReturn;
		}
		
		/// <summary>
		/// Get a particular value by key and hash, or null.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="hash"></param>
		/// <returns></returns>
		public string Get(ID key, ID hash)
		{
			lock(store) {
				if(this.Contains(key, hash)) {
					File.ReadAllText(PathFor(key, hash));
				}
			}
			return null;
		}
		
		/// <summary>
		/// Returns when the given value was last inserted by
		/// its original publisher, or null if the value isn't present.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public DateTime? GetPublicationTime(ID key, ID hash)
		{
			lock(store) {
				if(store.ContainsKey(key) && store[key].ContainsKey(hash)) {
					return store[key][hash].timestamp;
				}
			}
			return null;
		}
		
		/// <summary>
		/// Get all IDs, so we can go through and republish everything.
		/// It's a copy so you can iterate it all you want.
		/// </summary>
		public IList<ID> GetKeys()
		{
			List<ID> toReturn = new List<ID>();
			lock(store) {
				foreach(ID key in store.Keys) {
					toReturn.Add(key);
				}
			}
			return toReturn;
		}
		
		/// <summary>
		/// Gets a list of all value hashes for the given key
		/// It's a copy, iterate all you want.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IList<ID> GetHashes(ID key)
		{
			List<ID> toReturn = new List<ID>();
			lock(store) {
				if(store.ContainsKey(key)) {
					foreach(ID hash in store[key].Keys) {
						toReturn.Add(hash);
					}
				}
			}
			return toReturn;
		}
		
		/// <summary>
		/// Expire old entries
		/// </summary>
		public void Expire()
		{
			lock(store) {
				for(int i = 0; i < store.Count; i++) {
					// Go through every value for the key
					SortedList<ID, Entry> vals = store.Values[i];
					for(int j = 0; j < vals.Count; j++) {
						if(DateTime.Now.ToUniversalTime() 
						   > vals.Values[j].timestamp + vals.Values[j].keepFor) { // Too old!
							// Delete file
							string filePath = PathFor(store.Keys[i], vals.Keys[j]);
							File.Delete(filePath);
							
							// Remove index
							vals.RemoveAt(j);
							j--;
						}
					}
					
					// Don't keep empty value lists around, or their directories
					if(vals.Count == 0) {
						string keyPath = Path.Combine(storageRoot, store.Keys[i].ToPathString());
						Directory.Delete(keyPath);
						store.RemoveAt(i);
						i--;
					}
				}
			}
			// TODO: Remove files that the index does not mention!
		}
	}
}
