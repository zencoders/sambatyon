using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    public class KademliaKeyword: IDocumentType, ILoadable
    {
        public List<string> Tags
        {
            get;
            set;
        }

        public KademliaKeyword()
        {
            this.Id = null;
            this.Tags = new List<string>();
        }

        public KademliaKeyword(string key, params string[] tags)
            : this()
        {
            this.Id = key;
            this.Tags.AddRange(tags);
        }
        #region ILoadable 

        public dynamic GetAsDatabaseType()
        {
            return this;
        }

        public bool LoadFromDatabaseType(dynamic data)
        {
            this.Id = data.Id;
            Tags.Clear();
            this.Tags.AddRange(data.Tags);
            return true;
        }

        public Type GetDatabaseType()
        {
            return this.GetType();
        }

        #endregion

        #region IDocumentType 

        public string Id
        {
            get;
            set;
        }
        #endregion
    }
}
