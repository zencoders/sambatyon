using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Persistence.Tag;
using System.Runtime.Serialization;

namespace Persistence
{
    [Serializable]
    public class KademliaResource: IDocumentType,ILoadable,ISerializable
    {
        public KademliaResource()
        {
            this.Tag = null;
            this.Urls = new List<DhtElement>();

        }
        public KademliaResource(string filename, params DhtElement[] urls) :this() {
            this.Tag = new CompleteTag(filename);
            if (urls.Length != 0)
            {
                this.Urls.AddRange(urls);
            }
        }
        public KademliaResource(CompleteTag tag, params DhtElement[] urls) : this()
        {
            this.Tag = tag;
            if (urls.Length != 0)
            {
                this.Urls.AddRange(urls);
            }
        }
        public KademliaResource(SerializationInfo info, StreamingContext ctxt) : this()
        {
            this.Tag = (CompleteTag)info.GetValue("Tag", typeof(CompleteTag));
            this.Urls = (List<DhtElement>)info.GetValue("Urls", typeof(List<DhtElement>));
        }
        #region IDocumentType
        public string Id
        {
            get
            {
                if (this.Tag != null)
                {
                    return this.Tag.TagHash;
                }
                else
                {
                    return null;
                }

            }
            set
            {
            }
        }
        #endregion
        public CompleteTag Tag
        {
            get;
            set;
        }
        public List<DhtElement> Urls
        {
            get;
            set;
        }        
        #region ILoadable

        public dynamic GetAsDatabaseType()
        {
            return this;
        }

        public bool LoadFromDatabaseType(dynamic data)
        {
            this.Tag = data.Tag;
            this.Id = data.Id;
            this.Urls.Clear();
            this.Urls.AddRange(data.Urls);
            return true;
        }

        public Type GetDatabaseType()
        {
            return this.GetType();
        }
        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Tag", this.Tag);
            info.AddValue("Urls", this.Urls);
        }
    }
}
