using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    class DhtElementComparer : IEqualityComparer<DhtElement>
    {
        bool IEqualityComparer<DhtElement>.Equals(DhtElement x, DhtElement y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<DhtElement>.GetHashCode(DhtElement obj)
        {
            return obj.GetHashCode();
        }
    }
}
