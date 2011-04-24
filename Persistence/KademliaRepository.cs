using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Persistence.Tag;
using System.Threading.Tasks;
using System.Threading;

namespace Persistence
{
    public class KademliaRepository:IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(KademliaRepository));
        private Repository _repository;
        public KademliaRepository(string repType,RepositoryConfiguration conf) {
            this._repository = RepositoryFactory.GetRepositoryInstance(repType, conf);
        }
        public bool StoreResource(CompleteTag tag, Uri peer)
        {
            KademliaResource rs = new KademliaResource();
            RepositoryResponse resp = _repository.GetByKey<KademliaResource>(tag.TagHash, rs);
            if ( resp == RepositoryResponse.RepositoryLoad)
            {
                if (!rs.Urls.Contains(peer)) {
                    _repository.ArrayAddElement(rs.Id, "Urls", peer);
                } else {
                    log.Debug("Urls "+peer.ToString()+" already known");
                }
            }
            else if (resp == RepositoryResponse.RepositoryMissingKey)
            {
                rs = new KademliaResource(tag, peer);
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

        private string discardSemanticlessWords(string str)
        {
            return str;
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
            string[] keywords = discardSemanticlessWords(sb.ToString()).Split(' ');
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
                if (!pkList.Contains(key)) pkList.Add(@"kademliakeywords/"+key);
            }
            return pkList.ToArray();
        }


        #region IDisposable

        public void Dispose()
        {
            _repository.Dispose();
        }

        #endregion
    }
}
