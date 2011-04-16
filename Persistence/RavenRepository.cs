using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
namespace RepositoryImpl
{
    class RavenRepository : Repository
    {
        public RavenRepository(RepositoryConfiguration config)
        {
            this.RepositoryType = "Raven";
            Console.WriteLine(config.GetConfig("data_dir"));
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