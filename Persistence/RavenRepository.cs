using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Client;
using Raven.Storage;
using Raven.Client;
using System.Transactions;
using Raven.Client.Document;
using log4net;
using System.Threading.Tasks;
using Raven.Database.Json;
using Raven.Database.Data;
using Newtonsoft.Json.Linq;
using Raven.Database.Indexing;
using Raven.Client.Indexes;

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository : Repository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RavenRepository));
        //private EmbeddableDocumentStore _doc;        
        private DocumentStore _store;          
        public RavenRepository(RepositoryConfiguration config=null)
        {
            this.RepositoryType = "Raven";
            string dataDir = System.IO.Path.GetTempPath()+"/p2p-player-db";
            if (config != null)
            {
                string tdd = config.GetConfig("data_dir");
                if (!tdd.Equals("")) dataDir = tdd;
            }
            log.Debug("Start opening and initializing RavenDB");
            //_doc = new EmbeddableDocumentStore { DataDirectory = dataDir};
            _store = new DocumentStore { Url = "http://localhost:8080" };
            _store.Initialize();               
            log.Info("RavenDB initialized at " + dataDir);
        }        
        public override RepositoryResponse Save(ILoadable data)
        {
            try
            {
                using (TransactionScope tx = new TransactionScope())
                {
                    using (IDocumentSession _session = _store.OpenSession())
                    {
                        dynamic entity = data.GetAsDatabaseType();
                        _session.Store(entity);
                        _session.SaveChanges();
                        tx.Complete();
                        log.Debug("Data saved with id " + entity.Id);
                    }
                }
            }
            catch (TransactionAbortedException tae)
            {
                log.Error("Transaction aborted", tae);
                return RepositoryResponse.RepositoryTransactionAbort;
            }
            return RepositoryResponse.RepositorySuccess;
        }

        public override RepositoryResponse Delete<DBType>(string id)
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                var entity = _session.Load<DBType>(id);
                if (entity != null)
                {
                    try
                    {
                        using (TransactionScope tx = new TransactionScope())
                        {
                            _session.Delete<DBType>(entity);
                            _session.SaveChanges();
                            tx.Complete();
                            log.Debug("Data with id " + id + " deleted");
                        }
                        return RepositoryResponse.RepositoryDelete;
                    }
                    catch (TransactionAbortedException tae)
                    {
                        log.Error("Transaction Aborted", tae);
                        return RepositoryResponse.RepositoryGenericError;
                    }
                }
                else
                {
                    return RepositoryResponse.RepositoryGenericError;
                }
            }
        }
        public override RepositoryResponse BulkDelete<DBType>(string[] ids)
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                DBType[] ents = _session.Load<DBType>(ids);
                try
                {
                    using (TransactionScope tx = new TransactionScope())
                    {
                        foreach (DBType entity in ents)
                        {
                            _session.Delete<DBType>(entity);
                        }
                        _session.SaveChanges();
                        tx.Complete();
                    }
                    return RepositoryResponse.RepositoryDelete;
                }
                catch (TransactionAbortedException tae)
                {
                    log.Error("Transaction Aborted", tae);
                    return RepositoryResponse.RepositoryGenericError;
                }
            }
        }
 

        public override int Count<DBType>()
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                return _session.Query<DBType>().Count();
            }
        }

        public override RepositoryResponse GetByKey<DBType>(string id, ILoadable elem)
        {

            DBType entity;
            using (IDocumentSession _session = _store.OpenSession())
            {
                entity = _session.Load<DBType>(id);
            }
            if (entity != null)
            {
                if (elem.LoadFromDatabaseType(entity))
                {
                    log.Debug("Data with key " + id + " loaded");
                    return RepositoryResponse.RepositoryLoad;
                }
                else
                {
                    return RepositoryResponse.RepositoryGenericError;
                }
            }
            else
            {
                return RepositoryResponse.RepositoryMissingKey;
            }
        }

        public override RepositoryResponse GetAllByCondition<DBType>(Func<DBType, bool> cond,List<DBType> elems)
        {            
            using (IDocumentSession _session = _store.OpenSession())
            {
                var results = _session.Query<DBType>().AsParallel().Where(cond);
                elems.AddRange(results);
            }
            return RepositoryResponse.RepositoryLoad;
        }
        public override RepositoryResponse GetByKeySet<DBType>(string[] ids, List<DBType> elems)
        {
            if (elems != null)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    DBType[] ents = _session.Load<DBType>(ids);
                    elems.AddRange(ents);
                }
                return RepositoryResponse.RepositoryLoad;
            }
            else
            {
                return RepositoryResponse.RepositoryGenericError;
            }
        }
        public override RepositoryResponse GetAll<DBType>(ICollection<DBType> cont)
        {
            if (cont!=null) {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Parallel.ForEach(_session.Query<DBType>(), t =>
                    {
                        cont.Add(t);
                    });
                }
                return RepositoryResponse.RepositoryLoad;
            } else {
                return RepositoryResponse.RepositoryGenericError;
            }
        }
        public override RepositoryResponse CreateIndex(string indexName, string indexQuery)
        {
            IndexDefinition def = new IndexDefinition() { Map = indexQuery, Name = indexName };
            log.Debug(_store.DatabaseCommands.PutIndex(indexName, def));
            return RepositoryResponse.RepositorySuccess;
        }
        public override RepositoryResponse QueryOverIndex<DBType>(string indexName, string query,List<DBType> elems){
            if (elems == null) return RepositoryResponse.RepositoryGenericError;
            using (IDocumentSession _session = _store.OpenSession()) {
                var results = _session.Advanced.LuceneQuery<DBType>(indexName).Where(query);                                
                elems.AddRange(results);
            }
            return RepositoryResponse.RepositoryLoad;
        }
        private bool patchDatabase(string key, string propertyName, object value, PatchCommandType type)
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                    new[] {
                        new PatchCommandData {
                            Key = key,                        
                            Patches = new [] {
                                new PatchRequest {
                                    Type = type,
                                    Name = propertyName,                                    
                                    Value = JToken.FromObject(value)
                                }
                            }
                        }
                    }
                );
            }
            return true;
        }

        public override RepositoryResponse ArrayAddElement(string key, string property, object element)
        {
            patchDatabase(key, property, element, PatchCommandType.Add);
            return RepositoryResponse.RepositoryPatchAdd;
        }

        public override RepositoryResponse ArrayRemoveElement(string key, string property, object value)
        {
            patchDatabase(key, property, value, PatchCommandType.Remove);
            return RepositoryResponse.RepositoryPatchRemove;
        }

        public override RepositoryResponse SetPropertyValue(string key, string property, object newValue)
        {
            patchDatabase(key, property, newValue, PatchCommandType.Set);
            return RepositoryResponse.RepositoryPatchSet;
        }

        #region IDisposable

        public override void Dispose()
        {
            log.Debug("Disposing Raven Repository...");
            this._store.Dispose();
            log.Info("Raven Repository Disposed. Resource Released");
        }

        #endregion
    }
}
}