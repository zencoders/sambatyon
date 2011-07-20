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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Persistence.Tag;
using System.Threading.Tasks;
using System.Threading;
using Raven.Client.Indexes;
using Raven.Database.Indexing;
using Raven.Client.Document;
using System.Text.RegularExpressions;


namespace Persistence
{
    /// <summary>
    /// Class implementig a specialized repository that will be used by kademlia peer to store information need by the DHT.
    /// Kademlia repository use two distinct collection to implements the three tier representation of knowledge. 
    /// The first collection contains keywords which are small enough to be exchanged frequently on the network; each keyword has 
    /// a list of related tags. A tag is related to a keyword if this word is used in one or more of the following tag fields: title,
    /// author and album. There's a list of semanticless word that cannot be used as keyword.
    /// The second collection contains resources which are composed of complete tag information and a list of DHT elements (read 
    /// Kademlia node references). This resources are necessary for the Transport Layer in order to identify the location of possible
    /// suppliers for a given track.
    /// </summary>
    public class KademliaRepository:IDisposable
    {
        /// <summary>
        /// Constrant string that contains a regular expression used to find semanticless words: this regex detects articles, conjuction
        /// and prepositions for English,Italian and French language.
        /// </summary>
        public const string DefaultSemanticFilterRegexString=@"\b("+
                                                              "the|a|an|"+//English Articles
                                                              "for|and|nor|but|or|yet|so|of|to|" + //English Coordinating conjunction
                                                              "both|either|neither|rather|whatever|" + //English Correlative Conjunction (not all)
                                                              "as|although|for|if|so|than|unless|until|till|while|" +//English Subordinate Conjunctions (not all)
                                                              "lo|il|la|i|gli|le|l'|"+//Italian Articles
                                                              "di|a|da|in|con|su|per|fra|tra|"+//Italian Prepositions
                                                              "e|anche|pure|inoltre|ancora|perfino|neanche|neppure|nemmeno|"+ //Italian Conjunctions
                                                              "oppure|altrimenti|ovvero|ossia|dunque|quindi|pertanto|allora|infatti|difatti|invero|"+ //Italian Conjunctions (Continue)
                                                              "che|mentre|se|"+//Italian Conjunctions (Continue)
                                                              "un|une|des|de|"+ //French articles
                                                              "et|ou|ni|mais|donc"+ //French Conjunction (car omitted to avoid clash with english terms)
                                                              @")\b";
        /// <summary>
        /// Logger instance used for logging purpose.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(KademliaRepository));
        /// <summary>
        /// Repository used to store Kademlia information
        /// </summary>
        private Repository _repository;
        /// <summary>
        /// Compiled Regex used to remove semanticless words.
        /// </summary>
        private Regex _semanticRegex;
        /// <summary>
        /// Compiled Regex used to remove whitespaces.
        /// </summary>
        private Regex _whiteSpaceRegex;
        /// <summary>
        /// Fixed validity period for the Dht Elements
        /// </summary>
        private TimeSpan _elementValidity;
        /// <summary>
        /// Constructor for the Kademlia Repository. This begins with compiling regular expressions for whitespace and semanticless
        /// words and setting timespan sing the given values (if they are not passed, it uses the default). Then instantiates the 
        /// repository of the fiven type and creates two indexes over the instantiatied repository. The first index is used to 
        /// find keywords with an empty tag list; the second one is used to query keyword using tag identifier. Both indexes are 
        /// necessary in order to cleanly delete resources and keywords from the repository
        /// </summary>
        /// <param name="repType">Name of the repository type. The default repository type is RavenDB ("Raven")</param>
        /// <param name="conf">Repository Configureation</param>
        /// <param name="elementValidity">Validity period of a Dht Element. Default value is 24 hours (1 day). The validity must
        /// be expressed in timespan format as described in MSDN reference for this type.</param>
        /// <param name="semanticFilter">Regular expression that will be used to remove semanticless word.</param>
        public KademliaRepository(string repType="Raven",
                                  RepositoryConfiguration conf=null,
                                  string elementValidity = "1",
                                  string semanticFilter=KademliaRepository.DefaultSemanticFilterRegexString) 
        {
            log.Debug("Semantic Filter Regex used is "+DefaultSemanticFilterRegexString);
            if (!(TimeSpan.TryParse(elementValidity, out this._elementValidity)))
            {
                this._elementValidity = new TimeSpan(24, 0, 0);
            }
            this._semanticRegex = new Regex(DefaultSemanticFilterRegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this._whiteSpaceRegex = new Regex(@"[ ]{2,}", RegexOptions.Compiled);
            this._repository = RepositoryFactory.GetRepositoryInstance(repType, conf);
            this._repository.CreateIndex("KademliaKeywords/KeysByTag",
                                         "from key in docs.KademliaKeywords\nfrom tag in key.Tags\nselect new { Kid = key.Id , Tid = tag}");
            this._repository.CreateIndex("KademliaKeywords/EmptyKeys",
                                         "from key in docs.KademliaKeywords\nwhere key.Tags.Count() == 0\nselect new { key.Id }");
        }
        /// <summary>
        /// Stores a tag as resource into the kademlia repository. If the resource already exists the method tries to add the 
        /// given Url (with the desired publication time) to the list of suppliers for that resource; if the url is already known
        /// the method does nothing. If the resource is new first the method adds it to the repository and then generates a set of
        /// keywords related to the new resource. For each generated keyword, if this is already in the repository the tag
        /// identifier (that is the resource identifier too) will be added to the related tags list, if the keyword doesn't exist
        /// it will be created and its related tags list will contains only the new resource.
        /// </summary>
        /// <param name="tag">Tag to store in the kademlia resource</param>
        /// <param name="peer">Url of the supplier</param>
        /// <param name="pubtime">Publication TIme</param>
        /// <returns>False if something went wrong, true otherwise</returns>
        public bool StoreResource(CompleteTag tag, Uri peer,DateTime pubtime)
        {
            KademliaResource rs = new KademliaResource();
            Console.WriteLine("Storing resource from peer " + peer);
            DhtElement dhtElem = new DhtElement(peer, pubtime, this._elementValidity);
            RepositoryResponse resp = _repository.GetByKey<KademliaResource>(tag.TagHash, rs);
            if ( resp == RepositoryResponse.RepositoryLoad)
            {
                if (!rs.Urls.Contains(dhtElem)) {
                    _repository.ArrayAddElement(rs.Id, "Urls", dhtElem);
                } else {
                    log.Debug("Urls "+peer.ToString()+" already known");
                }
            }
            else if (resp == RepositoryResponse.RepositoryMissingKey)
            {
                rs = new KademliaResource(tag, dhtElem);
                if (_repository.Save(rs) == RepositoryResponse.RepositorySuccess)
                {
                    List<string> pks = new List<string>(generatePrimaryKey(tag));
                    List<KademliaKeyword> keys = new List<KademliaKeyword>();
                    if (_repository.GetByKeySet(pks.ToArray(), keys) > 0)
                    {
                        foreach (KademliaKeyword k in keys)
                        {
                            if (!k.Tags.Contains(rs.Id))
                            {
                                _repository.ArrayAddElement(k.Id, "Tags", rs.Id);
                            }
                            pks.Remove(k.Id);
                        }
                        foreach (String pk in pks)
                        {
                            KademliaKeyword localKey = new KademliaKeyword(pk,rs.Id);
                            _repository.Save(localKey);                            
                        }
                    }
                    else
                    {
                        log.Error("Unexpected reposnde while getting keywords");
                        return false;
                    }

                }
                else
                {
                    log.Error("Unexpected response while inserting Tag with key " + tag.TagHash);
                    return false;
                }
            }
            else
            {
                log.Error("Unexpected response while testing presence of the key " + tag.TagHash);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Performs search query over the repository. This split query in pieces and search for keywords that contains those pieces.
        /// Then for each keyword the method loads all the related resource. 
        /// </summary>
        /// <param name="query">Query string</param>
        /// <returns>An array containing the resource found</returns>
        public KademliaResource[] SearchFor(string query)
        {
            List<KademliaResource> resources = new List<KademliaResource>();
            List<KademliaKeyword> keys = new List<KademliaKeyword>();
            string[] queryParts = query.Split(' ');
            _repository.GetAllByCondition(kw =>
            {
                string kid=kw.Id.Substring(17);
                foreach (string p in queryParts) {
                    if (kid.Contains(p.ToLower()))
                    {
                        return true;
                    }
                }
                return false;
            }, keys);
            List<string> tids = new List<string>();
            foreach (KademliaKeyword kw in keys)
            {
                tids.AddRange(kw.Tags);
            }
            _repository.GetByKeySet(tids.ToArray(), resources);
            return resources.ToArray();
        }
        /// <summary>
        /// Deletes a tag with a given identifier. This method finds all keywords containing a reference to the tag to be deleted and
        /// removes the tag identifier from the keyword's list; then it search for keywords that has an empty list and deletes them.
        /// At the end the method removes the resource from the repository
        /// </summary>
        /// <param name="tid">Identifier of the tag to be deleted</param>
        /// <returns>False if something went wrong, true otherwise</returns>
        public bool DeleteTag(string tid)
        {
            List<KademliaKeyword> results = new List<KademliaKeyword>();
            this._repository.QueryOverIndex("KademliaKeywords/KeysByTag", "Tid:"+tid, results);
            foreach (var t in results)
            {
                //t.Tags.FindIndex(x => x.Equals(tid))
                this._repository.ArrayRemoveElement(t.Id,"Tags",tid);
            }
            this._repository.Delete<KademliaResource>(tid);
            results.Clear();
            if (this._repository.QueryOverIndex("KademliaKeywords/EmptyKeys", "", results) != RepositoryResponse.RepositoryLoad)
            {
                return false;
            }
            string[] ids = new string[results.Count];
            int index = 0;
            foreach (var t in results)
            {
                ids[index++] = t.Id;
            }
            this._repository.BulkDelete<KademliaKeyword>(ids);
            return true;
        }
        /// <summary>
        /// This methods removes all duplicated white space and then removes all semanticless words.
        /// </summary>
        /// <param name="str">String to be cleaned</param>
        /// <returns>Cleaned String</returns>
        public string DiscardSemanticlessWords(string str)
        {
            return _whiteSpaceRegex.Replace(_semanticRegex.Replace(str,"")," ").Trim();
        }
        /// <summary>
        /// Generates a clean list (without semanticless words) of keyword related to the given tag
        /// </summary>
        /// <param name="tag">Tag for whom we want to generate the keywords</param>
        /// <returns>A clean array of related keywords</returns>
        private string[] generatePrimaryKey(CompleteTag tag)
        {
            List<string> pkList = new List<string>();            
            StringBuilder sb = new StringBuilder();
            sb.Append(tag.Title);
            sb.Append(" ");
            sb.Append(tag.Artist);
            sb.Append(" ");
            sb.Append(tag.Album);
            string[] keywords = DiscardSemanticlessWords(sb.ToString()).Split(' ');
            UTF8Encoding enc = new UTF8Encoding();
            //Questo è solo un esercizio di stile !
            /*Parallel.ForEach<string,List<string> >(keywords, 
                                                    () => new List<string>(),                                    
                                                    (key,loop,sublist)  =>
                                                    {
                                                        sublist.Add(key);
                                                        return sublist;
                                                    },
                                                    (finalResult) => pkList.AddRange(finalResult)
            );*/
            for (int k = 0; k < keywords.Length; k++)
            {
                string key = null;
                if (keywords[k].Length > 32)
                {
                    key = keywords[k].Substring(0, 32).ToLower();
                }
                else
                {
                    key = keywords[k].ToLower();
                }
                if (!pkList.Contains(key)) pkList.Add("kademliakeywords/"+key);
            }
            return pkList.ToArray();
        }
        /// <summary>
        /// Sets the publication time of a Dht Element in order to avoid expiration
        /// </summary>
        /// <param name="tagid">Resource Identifier</param>
        /// <param name="url">URL of the Dht Element to refresh</param>
        /// <param name="pubtime">Publication time to set</param>
        /// <returns>True if the Dht Element exists and has been refreshed, false otherwise</returns>
        public bool RefreshResource(string tagid, Uri url,DateTime pubtime)
        {
            KademliaResource rs = new KademliaResource();
            RepositoryResponse resp = _repository.GetByKey<KademliaResource>(tagid, rs);
            if (resp == RepositoryResponse.RepositoryLoad)
            {
                int eindex = rs.Urls.ToList().FindIndex(elem =>
                {
                    return elem.Url.Equals(url);
                });
                if (eindex != -1)
                {
                    _repository.ArraySetElement(tagid, "Urls", eindex, "Publication", pubtime);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the Resource identified by the given taghash.
        /// </summary>
        /// <param name="tagid">Identifier of the resource to get</param>
        /// <returns>The requested resource if it exists, null if not.</returns>
        public KademliaResource Get(string tagid)
        {
            KademliaResource rs = new KademliaResource();
            if (_repository.GetByKey<KademliaResource>(tagid, rs) == RepositoryResponse.RepositoryLoad)
            {
                return rs;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Returns a list of all resources of the repository.
        /// </summary>
        /// <returns>Linked List containing all the resources</returns>
        public LinkedList<KademliaResource> GetAllElements()
        {
            LinkedList<KademliaResource> coll=new LinkedList<KademliaResource>();
            if (_repository.GetAll<KademliaResource>(coll) == RepositoryResponse.RepositoryLoad)
            {
                return coll;
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// Method that implements the registration of a peer as supplier for a given resource.
        /// This method just load the resource and adds the Dht Element if it's not already in the suppliers list.
        /// </summary>
        /// <param name="tagid">Resource Identifier</param>
        /// <param name="url">Url of the supplier</param>
        /// <param name="pubtime">Publication Time for the supplier</param>
        /// <returns></returns>
        public bool Put(string tagid, Uri url, DateTime pubtime)
        {
            KademliaResource rs = new KademliaResource();
            DhtElement dhtElem = new DhtElement(url, pubtime, this._elementValidity);
            RepositoryResponse resp = _repository.GetByKey<KademliaResource>(tagid, rs);
            if (resp == RepositoryResponse.RepositoryLoad)
            {
                if (!rs.Urls.Contains(dhtElem))
                {
                    _repository.ArrayAddElement(rs.Id, "Urls", dhtElem);
                    return true;
                }
                else
                {
                    log.Debug("Urls " + url.ToString() + " already known");
                }                
            }
            return false;
        }
        /// <summary>
        /// Checks if the repository contains a given tag
        /// </summary>
        /// <param name="tagid">Resource Identifier to search</param>
        /// <returns>True if the repository contains the resource, false if not.</returns>
        public bool ContainsTag(string tagid)
        {
            KademliaResource rs = Get(tagid); 
            if (rs != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks if the given resource contains the Url in its supplier list
        /// </summary>
        /// <param name="tagid">Resource identifier</param>
        /// <param name="url">Url of the supplier</param>
        /// <returns>True if the Peer with the given Url is a supplier for the resource.</returns>
        public bool ContainsUrl(string tagid, Uri url)
        {
            KademliaResource rs = Get(tagid);
            DhtElement fakeElem=new DhtElement() 
            {
                Url = url
            };
            if (rs != null)
            {
                if (rs.Urls.Contains(fakeElem))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the publication time of a Dht Element related to a Resource.
        /// </summary>
        /// <param name="tagid">Resource Identifier</param>
        /// <param name="url">Url of the Dht Element</param>
        /// <returns>The publication time of the Dht Element if it exists, the DateTime min value if the element doesn't exist</returns>
        public DateTime GetPublicationTime(string tagid, Uri url)
        {
            KademliaResource rs = Get(tagid);            
            if (rs!=null)
            {
                DhtElement elem = rs.Urls.ToList().Find(de =>
                    {
                        return de.Url.Equals(url);
                    }
                );
                if (elem != null)
                {
                    return elem.Publication;
                }
            }
            return DateTime.MinValue;
        }
        /// <summary>
        /// Method that removes the expired elements from the DHT. This method search for expired element and add it to a structure,
        /// then delete all the expired elements and if a resource remanins without any supplier will be deleted too.
        /// </summary>
        public void Expire()
        {
            List<KademliaResource> lr=new List<KademliaResource>();
            LinkedList<ExpireIteratorDesc> cleanList = new LinkedList<ExpireIteratorDesc>();
            _repository.GetAll<KademliaResource>(lr);
            Parallel.ForEach<KademliaResource,ExpireIteratorDesc >(lr, 
                                                    () => new ExpireIteratorDesc(),                                    
                                                    (key,loop,iter_index,iter_desc)  =>
                                                    {
                                                        if (key == null) return iter_desc;
                                                        if (iter_desc.TagId == null)
                                                        {
                                                            iter_desc.TagId = key.Id;
                                                        }
                                                        List<DhtElement> dhtElementList = key.Urls.ToList();
                                                        for (int k=0;k<dhtElementList.Count;k++)
                                                        {
                                                            DhtElement delem = dhtElementList[k];
                                                            if (DateTime.Compare(delem.Publication.Add(delem.Validity),DateTime.Now) <= 0)
                                                            {
                                                                iter_desc.Expired.Add(delem);
                                                            }
                                                        }
                                                        if (iter_desc.Expired.Count == key.Urls.Count)
                                                        {
                                                            iter_desc.ToBeDeleted= true;
                                                        }
                                                        else
                                                        {
                                                            iter_desc.ToBeDeleted = false;
                                                        }
                                                        return iter_desc;
                                                    },
                                                    (finalResult) => cleanList.AddLast(finalResult)
            );
            Parallel.ForEach<ExpireIteratorDesc>(cleanList,
                (iter_desc) =>
                {
                    if (iter_desc == null) return;
                    if (iter_desc.ToBeDeleted)
                    {
                        DeleteTag(iter_desc.TagId);
                    }
                    else
                    {
                        _repository.ArrayRemoveByPosition(iter_desc.TagId, "Urls", iter_desc.Expired.ToArray<DhtElement>());
                    }
                }
            );
        }
        /// <summary>
        /// Private class to store Expire method iteration information.
        /// </summary>
        private class ExpireIteratorDesc
        {
            /// <summary>
            /// Tag Identifier for the iteration
            /// </summary>
            public string TagId
            {
                get;
                set;
            }
            /// <summary>
            /// Flag that indicates that the tag has to be completely deleted
            /// </summary>
            public bool ToBeDeleted
            {
                get;
                set;
            }
            /// <summary>
            /// List of expired element
            /// </summary>
            public List<DhtElement> Expired
            {
                get;
                set;
            }
            /// <summary>
            /// Default constructor.
            /// </summary>
            public ExpireIteratorDesc()
            {
                TagId = null;
                ToBeDeleted = false;
                Expired = new List<DhtElement>();
            }
        }
        #region IDisposable
        /// <summary>
        /// Method used to dispose the repository
        /// </summary>
        public void Dispose()
        {
            _repository.Dispose();
        }

        #endregion
    }    
}
