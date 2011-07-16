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
using System.Runtime.InteropServices;

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository : Repository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RavenRepository));
        private EmbeddableDocumentStore _store;        
        //private DocumentStore _store;          
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
            _store = new EmbeddableDocumentStore { DataDirectory = dataDir};
            //_store = new DocumentStore { Url = "http://localhost:8080" };
            _store.Initialize();               
            log.Info("RavenDB initialized at " + dataDir);
        }        
        public override RepositoryResponse Save(ILoadable data)
        {
            try
            {
                lock (_store)
                {
                    using (TransactionScope tx = new TransactionScope())
                    {
                        using (IDocumentSession _session = _store.OpenSession())
                        {
                            dynamic entity = data.GetAsDatabaseType();
                            _session.Store(entity);
                            try
                            {
                                _session.SaveChanges();
                            }
                            catch (COMException comE)
                            {
                                log.Warn("Cannot save cause to interop error");
                            }
                            tx.Complete();
                            log.Debug("Data saved with id " + entity.Id);
                        }
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
            lock (_store)
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
        }
        public override RepositoryResponse BulkDelete<DBType>(string[] ids)
        {
            lock (_store)
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
        }
 

        public override int Count<DBType>()
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                return _session.Query<DBType>().Customize(x => { x.WaitForNonStaleResults(); }).Count();
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
                var results = _session.Query<DBType>().Customize(x=> {x.WaitForNonStaleResults();}).AsParallel().Where(cond);
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
                    Parallel.ForEach(_session.Query<DBType>().Customize(x => { x.WaitForNonStaleResults(); }), t =>
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
            lock (_store)
            {
                IndexDefinition def = new IndexDefinition() { Map = indexQuery, Name = indexName };
                //_store.
                if (_store.DatabaseCommands.GetIndex(indexName) == null)
                {
                    log.Debug(_store.DatabaseCommands.PutIndex(indexName, def));
                }
            }
            return RepositoryResponse.RepositorySuccess;
        }
        public override RepositoryResponse QueryOverIndex<DBType>(string indexName, string query,List<DBType> elems){
            if (elems == null) return RepositoryResponse.RepositoryGenericError;
            if (this._store.DatabaseCommands.GetIndex(indexName) == null) return RepositoryResponse.RepositoryMissingIndex;
            using (IDocumentSession _session = _store.OpenSession()) {
                var results = _session.Advanced.LuceneQuery<DBType>(indexName).WaitForNonStaleResults().Where(query);                                
                elems.AddRange(results);
            }
            return RepositoryResponse.RepositoryLoad;
        }
        private bool patchDatabase(string key, string propertyName, object value, PatchCommandType type)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] {
                        new PatchCommandData {
                            Key = key,                        
                            Patches = new [] {
                                generatePatchRequest(propertyName,value,type)
                            }
                        }
                    }
                    );
                }
            }
            return true;
        }
        private PatchRequest generatePatchRequest(string propertyName, object value, PatchCommandType type)
        {
            return new PatchRequest {
                Type = type,
                Name = propertyName,
                Value = JToken.FromObject(value)
            };
        }
        private PatchRequest generatePatchRequest(string propertyName, int pos, PatchCommandType type)
        {
            return new PatchRequest {
                Type = type,
                Name = propertyName,
                Position = pos,
                Value = null
            };
        }
        private bool mupliplePatchDatabase(string key,params PatchRequest[] reqs)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] 
                    {
                        new PatchCommandData 
                        {
                            Key = key,
                            Patches = reqs
                        }
                    }
                    );
                }
            }
            return true;
        }
        private bool nestedPatchDatabase(string key,string propertyName, int index, params PatchRequest[] reqs)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] 
                    {
                        new PatchCommandData 
                        {
                            Key = key,
                            Patches = new [] 
                            {
                                new PatchRequest 
                                {
                                    Type = PatchCommandType.Modify,
                                    Name = propertyName,
                                    Position = index,
                                    Nested = reqs
                                }
                            }
                        }
                    }
                    );
                }
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

        public override RepositoryResponse ArrayRemoveByPosition(string key,string property,params object[] values)
        {
            PatchRequest[] reqs = new PatchRequest[values.Length];
            for (int k = 0; k < values.Length; k++)
            {
                reqs[k] = generatePatchRequest(property, values[k], PatchCommandType.Remove);
            }
            mupliplePatchDatabase(key, reqs);
            return RepositoryResponse.RepositoryPatchRemove;
        }

        public override RepositoryResponse ArraySetElement(string key, string property, int index, string obj_prop, object value)
        {
            PatchRequest req = generatePatchRequest(obj_prop, value, PatchCommandType.Set);
            nestedPatchDatabase(key, property, index, req);
            return RepositoryResponse.RepositoryPatchModify;
        }
               
        public override RepositoryResponse SetPropertyValue(string key, string property, object newValue)
        {
            patchDatabase(key, property, newValue, PatchCommandType.Set);
            return RepositoryResponse.RepositoryPatchSet;
        }

        #region IDisposable

        public override void Dispose()
        {
            lock (_store)
            {
                log.Debug("Disposing Raven Repository...");
                this._store.Dispose();
                log.Info("Raven Repository Disposed. Resource Released");
            }
        }

        #endregion
    }
}
}