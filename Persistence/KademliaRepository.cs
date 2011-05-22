using System;
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
    public class KademliaRepository:IDisposable
    {
        public const string DefaultSemanticFilterRegexString=@"\b("+
                                                              "the|a|an|"+//English Articles
                                                              "for|and|nor|but|or|yet|so|of|to|" + //English Coordinating conjunction
                                                              "both|either|neither|rather|whetever|" + //English Correlative Conjunction (not all)
                                                              "as|although|for|if|so|than|unless|until|till|while|" +//English Subordinate Conjunctions (not all)
                                                              "lo|il|la|i|gli|le|l'|"+//Italian Articles
                                                              "di|a|da|in|con|su|per|fra|tra|"+//Italian Prepositions
                                                              "e|anche|pure|inoltre|ancora|perfino|neanche|neppure|nemmeno|"+ //Italian Conjunctions
                                                              "oppure|altrimenti|ovvero|ossia|dunque|quindi|pertanto|allora|infatti|difatti|invero|"+ //Italian Conjunctions (Continue)
                                                              "che|mentre|se|"+//Italian Conjunctions (Continue)
                                                              "un|une|des|de|"+ //French articles
                                                              "et|ou|ni|mais|donc"+ //French Conjunction (car omitted to avoid clash with english terms)
                                                              @")\b";
        private static readonly ILog log = LogManager.GetLogger(typeof(KademliaRepository));
        private Repository _repository;
        private Regex _semanticRegex;
        private Regex _whiteSpaceRegex;
        private TimeSpan _elementValidity;
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
        public bool StoreResource(CompleteTag tag, Uri peer,DateTime pubtime)
        {
            KademliaResource rs = new KademliaResource();
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
        public string DiscardSemanticlessWords(string str)
        {
            return _whiteSpaceRegex.Replace(_semanticRegex.Replace(str,"")," ").Trim();
        }
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
        public bool RefreshResource(string tagid, Uri url,DateTime pubtime)
        {
            KademliaResource rs = new KademliaResource();
            RepositoryResponse resp = _repository.GetByKey<KademliaResource>(tagid, rs);
            if (resp == RepositoryResponse.RepositoryLoad)
            {
                int eindex = rs.Urls.FindIndex(elem =>
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
        public bool ContainsTag(string tagid)
        {
            KademliaResource rs = Get(tagid); 
            if (rs != null)
            {
                return true;
            }
            return false;
        }
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
        public DateTime GetPublicationTime(string tagid, Uri url)
        {
            KademliaResource rs = Get(tagid);            
            if (rs!=null)
            {
                DhtElement elem = rs.Urls.Find(de =>
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
        public void Expire()
        {
            List<KademliaResource> lr=new List<KademliaResource>();
            LinkedList<ExpireIteratorDesc> cleanList = new LinkedList<ExpireIteratorDesc>();
            _repository.GetAll<KademliaResource>(lr);
            Parallel.ForEach<KademliaResource,ExpireIteratorDesc >(lr, 
                                                    () => new ExpireIteratorDesc(),                                    
                                                    (key,loop,iter_index,iter_desc)  =>
                                                    {
                                                        if (iter_desc.TagId == null)
                                                        {
                                                            iter_desc.TagId = key.Id;
                                                        }
                                                        for (int k=0;k<key.Urls.Count;k++)
                                                        {
                                                            DhtElement delem = key.Urls[k];
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
        private class ExpireIteratorDesc
        {
            public string TagId
            {
                get;
                set;
            }
            public bool ToBeDeleted
            {
                get;
                set;
            }
            public List<DhtElement> Expired
            {
                get;
                set;
            }
            public ExpireIteratorDesc()
            {
                TagId = null;
                ToBeDeleted = false;
                Expired = new List<DhtElement>();
            }
        }
        #region IDisposable

        public void Dispose()
        {
            _repository.Dispose();
        }

        #endregion
    }    
}
