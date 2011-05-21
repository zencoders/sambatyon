using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    public class DhtElement: IEquatable<DhtElement>,IEquatable<Uri> 
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
    }
}
