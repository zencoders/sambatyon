using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Persistence
{
    [Serializable]
    public class DhtElement: IEquatable<DhtElement>,IEquatable<Uri>, ISerializable
    {
        #region Properties
        public Uri Url
        {
            get;
            set;
        }
        public DateTime Publication
        {
            get;
            set;
        }
        public TimeSpan Validity
        {
            get;
            set;
        }
        #endregion
        public DhtElement()
        {
        }
        public DhtElement(Uri u, DateTime p, TimeSpan v) :
            this()
        {
            this.Url = u;
            this.Publication = p;
            this.Validity = v;
        }

        public DhtElement(SerializationInfo info, StreamingContext ctxt)
            : this()
        {
            this.Url = (Uri) info.GetValue("url", typeof(Uri));
            this.Publication = (DateTime) info.GetValue("publication", typeof(DateTime));
            this.Validity = (TimeSpan) info.GetValue("validity", typeof(TimeSpan));
        }


        #region IEquatable<DhtElement>

        public bool Equals(DhtElement other)
        {
            return other.Url.Equals(this.Url);
        }

        #endregion

        #region IEquatable<Uri>

        public bool Equals(Uri other)
        {
            return other.Equals(this.Url);
        }

        #endregion

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("url", this.Url);
            info.AddValue("publication", this.Publication);
            info.AddValue("validity", this.Validity);
        }
    }
}
