using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TransportService.Messages
{
    [DataContract]
    public class GenericMessage : AbstractMessage
    {
        [DataMember]
        public override string MessageType
        {
            get;
            set;
        }

        [DataMember]
        public override string RID
        {
            get;
            set;
        }

        [DataMember]
        public override int CID
        {
            get;
            set;
        }

        [DataMember]
        public override Uri SenderAddress
        {
            get;
            set;
        }
    }
}
