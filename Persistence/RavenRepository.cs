using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Client;

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository : Repository
    {
        private EmbeddableDocumentStore _doc;
        public RavenRepository(RepositoryConfiguration config)
        {
            this.RepositoryType = "Raven";
            string c = config.GetConfig("data_dir");
            if (c.Equals(""))
            {
                throw new ArgumentException("Missing data_dir configuration!");
            } else {

        }
        public override RepositoryResponse Save(ILoadable data)
        {
            throw new NotImplementedException();
        }

        public override RepositoryResponse Delete(ILoadable elem)
        {
            throw new NotImplementedException();
        }

        public override int count()
        {
            throw new NotImplementedException();
        }

        public override RepositoryResponse GetByKey(string id, out ILoadable elem)
        {
            throw new NotImplementedException();
        }

        public override RepositoryResponse GetAll(out ICollection<ILoadable> cont)
        {
            throw new NotImplementedException();
        }
    }
}
}