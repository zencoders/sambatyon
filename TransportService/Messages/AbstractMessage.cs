using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TransportService.Messages
{
    [DataContract]
    public abstract class AbstractMessage
    {
        protected string messageType;
        protected string rID;
        protected int cID;
        protected Uri senderAddress;

        [DataMember]
        public abstract string MessageType
        {
            get;
            set;
        }

        [DataMember]
        public abstract string RID
        {
            get;
            set;
        }

        [DataMember]
        public abstract int CID
        {
            get;
            set;
        }

        [DataMember]
        public abstract Uri SenderAddress
        {
            get;
            set;
        }
    }
}
