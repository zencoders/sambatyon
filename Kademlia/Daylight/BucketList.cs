/*
 * Created by SharpDevelop.
 * User: anovak
 * Date: 6/22/2010
 * Time: 8:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Daylight
{
	/// <summary>
	/// A list of contacts.
	/// Also responsible for storing last lookup times for buckets, so we can refresh them.
	/// Not thread safe for multiple people writing at once, since you can't enforce preconditions.
	/// </summary>
	public class BucketList
	{
		private const int BUCKET_SIZE = 20; // "K" in the spec
		private const int NUM_BUCKETS = 8 * ID.ID_LENGTH; // One per bit in an ID
		
		private List<List<Contact>> buckets;
		private List<DateTime> accessTimes; // last bucket write or explicit touch
		private ID ourID;
		
		/// <summary>
		/// Make a new bucket list, for holding node contacts.
		/// </summary>
		/// <param name="ourID">The ID to center the list on.</param>
		public BucketList(ID ourID)
		{
			this.ourID = ourID;
			buckets = new List<List<Contact>>(NUM_BUCKETS);
			accessTimes = new List<DateTime>();
			
			// Set up each bucket
			for(int i = 0; i < NUM_BUCKETS; i++) {
				buckets.Add(new List<Contact>(BUCKET_SIZE));
				accessTimes.Add(default(DateTime));
			}
		}
		
		/// <summary>
		/// Returns what contact is blocking insertion (least promoted), or null if no contact is.
		/// </summary>
		/// <param name="toAdd"></param>
		/// <returns></returns>
		public Contact Blocker(ID toAdd)
		{
			int bucket = BucketFor(toAdd);
			lock(buckets[bucket]) { // Nobody can move it while we're getting it
				if(buckets[bucket].Count < BUCKET_SIZE) {
					return null;
				} else {
					return buckets[bucket][0];
				}
			}
		}
		
		/// <summary>
		/// See if we have a contact with the given ID.
		/// </summary>
		/// <param name="toCheck"></param>
		/// <returns></returns>
		public bool Contains(ID toCheck)
		{
			return this.Get(toCheck) != null;
		}
		
		/// <summary>
		/// Add the given contact at the end of its bucket.
		/// PRECONDITION: Won't over-fill bucket.
		/// </summary>
		/// <param name="toAdd"></param>
		public void Put(Contact toAdd)
		{
			if(toAdd == null) {
				return; // Don't be silly.
			}
			
			int bucket = BucketFor(toAdd.GetID());
			buckets[bucket].Add(toAdd); // No lock: people can read while we do this.
			lock(accessTimes) {
				accessTimes[bucket] = DateTime.Now;
			}
		}
		
		/// <summary>
		/// Report that a lookup was done for the given key.
		/// Key must not match our ID.
		/// </summary>
		/// <param name="key"></param>
		public void Touch(ID key)
		{
			lock(accessTimes) {
				accessTimes[BucketFor(key)] = DateTime.Now;
			}
		}
		
		/// <summary>
		/// Return the contact with the given ID, or null if it's not found.
		/// </summary>
		/// <param name="toGet"></param>
		/// <returns></returns>
		public Contact Get(ID toGet) {
			int bucket = BucketFor(toGet);
			lock(buckets[bucket]) { // Nobody can move it while we're getting it
				for(int i = 0; i < buckets[bucket].Count; i++) {
					if(buckets[bucket][i].GetID() == toGet) {
						return buckets[bucket][i];
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Return how many contacts are cached.
		/// </summary>
		/// <returns></returns>
		public int GetCount()
		{
			int found = 0;
			
			// Just enumerate all the buckets and sum counts
			for(int i = 0; i < NUM_BUCKETS; i++) {
				found = found + buckets[i].Count;
			}
			
			return found;
		}
		
		/// <summary>
		/// Move the contact with the given ID to the front of its bucket.
		/// </summary>
		/// <param name="toPromote"></param>
		public void Promote(ID toPromote)
		{
			Contact promotee = Get(toPromote);
			int bucket = BucketFor(toPromote);
			
			lock(buckets[bucket]) { // Nobody can touch it while we move it.
				buckets[bucket].Remove(promotee); // Take out
				buckets[bucket].Add(promotee); // And put in at end
			}
			
			lock(accessTimes) {
				accessTimes[bucket] = DateTime.Now;
			}
		}
		
		/// <summary>
		/// Remove a contact.
		/// </summary>
		/// <param name="toRemove"></param>
		public void Remove(ID toRemove)
		{
			int bucket = BucketFor(toRemove);
			lock(buckets[bucket]) { // Nobody can move it while we're removing it
				for(int i = 0; i < buckets[bucket].Count; i++) {
					if(buckets[bucket][i].GetID() == toRemove) {
						buckets[bucket].RemoveAt(i);
						return;
					}
				}
			}
		}
		
		/// <summary>
		/// Return a list of the BUCKET_SIZE contacts with IDs closest to 
		/// target, not containing any contacts with the excluded ID. 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="excluded"></param>
		/// <returns></returns>
		public List<Contact> CloseContacts(ID target, ID excluded)
		{
			return CloseContacts(NUM_BUCKETS, target, excluded);
		}
		
		/// <summary>
		/// Returns a list of the specified number of contacts with IDs closest 
		/// to the given key, excluding the excluded ID.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="target"></param>
		/// <param name="excluded"></param>
		/// <returns></returns>
		public List<Contact> CloseContacts(int count, ID target, ID excluded)
		{
			// These lists are sorted by distance.
			// Closest is first.
			List<Contact> found = new List<Contact>();
			List<ID> distances = new List<ID>();
			
			// For every Contact we have
			for(int i = 0; i < NUM_BUCKETS; i++) {
				lock(buckets[i]) {
					for(int j = 0; j < buckets[i].Count; j++) {
						Contact applicant = buckets[i][j];
						
						// Exclude excluded contact
						if(applicant.GetID() == excluded) {
							continue;
						}
						
						// Add the applicant at the right place
						ID distance = applicant.GetID() ^ target;
						int addIndex = 0;
						while(addIndex < distances.Count && distances[addIndex] < distance) {
							addIndex++;
						}
						distances.Insert(addIndex, distance);
						found.Insert(addIndex, applicant);
						
						// Remove the last entry if we've grown too big
						if(distances.Count >= count) {
							distances.RemoveAt(distances.Count - 1);
							found.RemoveAt(found.Count - 1);
						}
					}
				}
			}
			
			// Give back the list of closest.
			return found;
		}
		
		/// <summary>
		/// Return the number of nodes in the network closer to the key than us.
		/// This is a guess as described at http://xlattice.sourceforge.net/components/protocol/kademlia/specs.html
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int NodesToKey(ID key) {
			int j = BucketFor(key);
			
			// Count nodes in earlier buckets
			int inEarlierBuckets = 0;
			for(int i = 0; i < j; i++) {
				inEarlierBuckets += buckets[i].Count;
			}
			
			// Count closer nodes in actual bucket
			int inActualBucket = 0;
			lock(buckets[j]) {
				foreach(Contact c in buckets[j]) {
					if((c.GetID() ^ ourID) < (key ^ ourID)) { // Closer to us than key
						inActualBucket++;
					}
				}
			}
			
			return inEarlierBuckets + inActualBucket;
		}
		
		/// <summary>
		/// Returns what bucket an ID maps to.
		/// PRECONDITION: ourID not passed.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private int BucketFor(ID id) 
		{
			return(ourID.DifferingBit(id));
		}
		
		/// <summary>
		/// Return an ID that belongs in the given bucket.
		/// </summary>
		/// <param name="bucket"></param>
		/// <returns></returns>
		private ID ForBucket(int bucket)
		{
			// The same as ours, but differ at the given bit and be random past it.
			return(ourID.RandomizeBeyond(bucket));
		}
		
		/// <summary>
		/// A ToString for debugging.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string toReturn = "BucketList:";
			for(int i = 0; i < NUM_BUCKETS; i++) {
				List<Contact> bucket = buckets[i];
				lock(bucket) {
					if(bucket.Count > 0) {
						toReturn += "\nBucket " + i.ToString() + ":";
					}
					foreach(Contact c in bucket) {
						toReturn += "\n" + c.GetID().ToString() + "@" + c.GetEndPoint().ToString();
					}
				}
			}
			return toReturn;
		}
		
		/// <summary>
		/// Gets a list of IDs that fall in buckets that haven't been written to in tooOld.
		/// </summary>
		/// <param name="tooOld"></param>
		/// <returns></returns>
		public IList<ID> IDsForRefresh(TimeSpan tooOld)
		{
			List<ID> toReturn = new List<ID>();
			lock(accessTimes) {
				for(int i = 0; i < NUM_BUCKETS; i++) {
					if(DateTime.Now > accessTimes[i].Add(tooOld)) { // Bucket is old
						toReturn.Add(ourID.RandomizeBeyond(i)); // Make a random ID in the bucket to look up
					}
				}
			}
			return toReturn;
		}
	}
}
