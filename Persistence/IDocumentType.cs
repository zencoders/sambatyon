using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    public interface IDocumentType
    {
        string Id
        {
            get;
            set;
        }
    }
}
