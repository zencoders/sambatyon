using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Client;
using Raven.Storage;
using Raven.Client;
using System.Transactions;
using Raven.Client.Document;

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository<DBType> : Repository<DBType> where DBType : IDocumentType
    {
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
                Console.Write("[Debug] Opening and initializing RavenDb ... ");
                //_doc = new EmbeddableDocumentStore { DataDirectory = dataDir};
                _store = new DocumentStore { Url = "http://localhost:8080" };
                _store.Initialize();
                _session = _store.OpenSession();
                Console.WriteLine("done");
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
                    Console.WriteLine("[Debug] Data saved with id " + entity.Id);
                }
            }
            catch (TransactionAbortedException tae)
            {
                Console.WriteLine(tae.Message);
                return RepositoryResponse.RepositoryTransactionAbort;
            }
            return RepositoryResponse.RepositorySuccess;
        }

        public override RepositoryResponse Delete(string id)
        {
            var entity = _session.Load<DBType>(id);
            if (entity != null)
            {
                using (TransactionScope tx = new TransactionScope())
                {
                    _session.Delete<DBType>(entity);
                    _session.SaveChanges();
                    tx.Complete();
                }
                return RepositoryResponse.RepositoryDelete;
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
            Console.WriteLine("SEEKING FOR "+id);
            var entity = _session.Load<DBType>(id);
            if (entity != null && elem.LoadFromDatabaseType(entity))
            {
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
                foreach (DBType t in _session.Query<DBType>())
                {
                    cont.Add(t);
                }
                return RepositoryResponse.RepositoryLoad;
            } else {
                return RepositoryResponse.RepositoryGenericError;
            }
        }

        #region IDisposable

        public override void Dispose()
        {
            this._session.Dispose();
        }

        #endregion
    }
}
}