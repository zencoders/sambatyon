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

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository<DBType> : Repository<DBType> where DBType : IDocumentType
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RavenRepository<>));
        //private EmbeddableDocumentStore _doc;
        private DocumentStore _store;
        private IDocumentSession _session;
        public RavenRepository(RepositoryConfiguration config)
        {
            this.RepositoryType = "Raven";
            string dataDir = config.GetConfig("data_dir");
            if (dataDir.Equals(""))
            {
                throw new ArgumentException("Missing data_dir configuration!");
            }
            else
            {
                log.Debug("Start opening and initializing RavenDB");
                //_doc = new EmbeddableDocumentStore { DataDirectory = dataDir};
                _store = new DocumentStore { Url = "http://localhost:8080" };
                _store.Initialize();
                _session = _store.OpenSession();
                log.Info("RavenDB initialized with at " + dataDir + " and a session has been opened");
            }
        }        
        public override RepositoryResponse Save(ILoadable data)
        {
            try
            {
                using (TransactionScope tx = new TransactionScope())
                {
                    dynamic entity = data.GetAsDatabaseType();
                    _session.Store(entity);
                    _session.SaveChanges();
                    tx.Complete();
                    log.Debug("Data saved with id " + entity.Id);
                }
            }
            catch (TransactionAbortedException tae)
            {
                log.Error("Transaction aborted", tae);
                return RepositoryResponse.RepositoryTransactionAbort;
            }
            return RepositoryResponse.RepositorySuccess;
        }

        public override RepositoryResponse Delete(string id)
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

        public override int Count()
        {
            return _session.Query<DBType>().Count();
        }

        public override RepositoryResponse GetByKey(string id, ILoadable elem)
        {
            log.Debug("Seeking data with key " + id);
            var entity = _session.Load<DBType>(id);
            if (entity != null && elem.LoadFromDatabaseType(entity))
            {
                log.Debug("Data with key " + id + " loaded");
                return RepositoryResponse.RepositoryLoad;
            }
            else
            {
                return RepositoryResponse.RepositoryGenericError;
            }
        }

        public override RepositoryResponse GetAll(ICollection<DBType> cont)
        {
            if (cont!=null) {
                Parallel.ForEach(_session.Query<DBType>(), t =>
                {
                    cont.Add(t);
                });
                return RepositoryResponse.RepositoryLoad;
            } else {
                return RepositoryResponse.RepositoryGenericError;
            }
        }

        #region IDisposable

        public override void Dispose()
        {
            log.Debug("Disposing Raven Repository...");
            this._session.Dispose();
            this._store.Dispose();
            log.Info("Raven Repository Disposed. Resource Released");
        }

        #endregion
    }
}
}