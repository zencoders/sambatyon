using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    public interface ILoadable
    {
        dynamic GetAsDatabaseType();
        bool LoadFromDatabaseType(dynamic data);
        Type GetDatabaseType();
    }
}
