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
            get { return messageType; }
            set { messageType = value; }
        }

        [DataMember]
        public override string RID
        {
            get { return rID; }
            set { rID = value; }
        }

        [DataMember]
        public override int CID
        {
            get { return cID; }
            set { cID = value; }
        }

        [DataMember]
        public override Uri SenderAddress
        {
            get { return senderAddress; }
            set { senderAddress = value; }
        }
    }
}
